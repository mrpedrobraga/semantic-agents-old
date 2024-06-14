using Godot;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SkillDefinition;
using System;

#nullable enable

namespace GDSK;

/** <summary>Class that connects an agent with an AI provider.</summary> */
public partial class SKGateway : Resource
{
    [Export]
    public bool USE_AZURE_SERVICES = false;
    [Export]
    public string AZURE_ENDPOINT = "";
    [Export]
    public bool USE_HUGGING_FACE = false;
    [Export]
    public string GENERATION_MODEL = "text-davinci-003";
    [Export]
    public string EMBEDDING_MODEL = "text-embedding-ada-002";
    [Export]
    public string API_KEY = "[REDACTED]";
    [Export(PropertyHint.Dir)]
    public string SKILLS_DIRECTORY = ProjectSettings.GlobalizePath("res://addons/ai_semantic_agents/skills/");

    public IKernel? kernel;
    public SequentialPlanner? planner;
    public SKContext? InternalContext;
    public ITextEmbeddingGeneration? embeddingGenerator;
    public void Init(SKAgent agent, string backstory)
    {
        var builder = new KernelBuilder();

        if (USE_AZURE_SERVICES)
        {
            builder.WithAzureTextEmbeddingGenerationService(
                    GENERATION_MODEL,
                    AZURE_ENDPOINT,
                    API_KEY);
            builder.WithAzureTextCompletionService(
                    EMBEDDING_MODEL,
                    AZURE_ENDPOINT,
                    API_KEY);
        }
        else
        {
            builder.WithOpenAITextEmbeddingGenerationService(
                    EMBEDDING_MODEL,
                    API_KEY);
            builder.WithOpenAITextCompletionService(
                    GENERATION_MODEL,
                    API_KEY);
        }
        builder.WithMemoryStorage(new VolatileMemoryStore());

        kernel = builder.Build();

        planner = new SequentialPlanner(kernel);
        embeddingGenerator = kernel.GetService<ITextEmbeddingGeneration>();

        InternalContext = kernel.CreateNewContext();

        ImportBuiltinSkills();

        InternalContext["history"] = "";
        InternalContext["commands"] = "";
        InternalContext["agent_wants"] = "";

        InternalContext[TextMemorySkill.CollectionParam] = "agentInfo";
        InternalContext[TextMemorySkill.RelevanceParam] = "0.3";
    }

    private void ImportBuiltinSkills()
    {
        if (kernel is null) throw new Exception("Gateway was not initialized. Call Init() first.");

        kernel.ImportSkill(new TextMemorySkill(kernel.Memory));
    }

    public ISKFunction? RegisterCustomFunction(
        Delegate nativeFunction,
        string? skillName,
        string? functionName,
        string? description,
        System.Collections.Generic.IEnumerable<ParameterView>? parameters
    )
    {
        if (kernel is null) throw new Exception("Gateway was not initialized. Call Init() first.");

        return kernel.RegisterCustomFunction(SKFunction.FromNativeFunction(
            nativeFunction: nativeFunction,
            skillName: skillName,
            functionName: functionName,
            description: description,
            parameters: parameters
        ));
    }
    public async Task<Plan> RequestDecision(string input, Godot.Collections.Dictionary world_state)
    {
        if (InternalContext is null || planner is null) throw new Exception("Gateway was not initialized. Call Init() first.");

        InternalContext["world_state"] = world_state.ToString();

        // Generate plan;
        var plan = await planner.CreatePlanAsync(
            input
        );

        return plan;
    }
}
