using System;
using System.Threading.Tasks;
using Godot;
//using Godot.Collections;
using System.Collections.Generic;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Orchestration;

namespace GDSK;

/** <summary>Node that can generate sophisticated AI-powered behaviour for entities.</summary> */
[GlobalClass]
public partial class SKAgent : Node
{
    /** <summary>The connection with the Semantic Kernel SDK.</summary> */
    [Export]
    public SKGateway Gateway;

    /** <summary>The NPC's backstory.</summary> */
    [Export(PropertyHint.MultilineText)]
    public string Backstory;

    /** Whether this agent is ready.*/
    public bool IsReady;

    /** Whether the last ran plan was successful. */
    public bool PlanExecutionWasSuccessful = true;

    [Signal]
    public delegate void BecameReadyEventHandler();

    [Signal]
    public delegate void PlanGeneratedEventHandler();

    [Signal]
    public delegate void PlanFinishedEventHandler(String result);

    [Signal]
    public delegate void PlanErroredEventHandler(String result);

    public override void _Ready()
    {
        Gateway = new SKGateway();
        Gateway.Init(this, Backstory);
        IsReady = true;
        EmitSignal(SignalName.BecameReady);
        GD.Print("Gateway Ready.");
    }

    /** <summary>Register a callback as a command the agent can run in the game world.</summary> */
    public void RegisterFunction(
        Callable nativeFunction,
        string skillName,
        string description,
        Godot.Collections.Array<Godot.Collections.Dictionary> parameters
    )
    {
        GD.Print("Registering " + skillName + "::" + nativeFunction.Method);
        List<ParameterView> p_list = new();
        foreach (Godot.Collections.Dictionary parameter in parameters)
        {
            var p_name = parameter["name"].ToString();
            var p_description = parameter.GetValueOrDefault("description", "").ToString();
            var p_defaultValue = parameter.GetValueOrDefault("defaultValue", "").ToString();

            p_list.Add(new ParameterView(name: p_name, description: p_description, defaultValue: p_defaultValue));
        }
        string Wrapper(string dx, string dy)
        {
            var result = nativeFunction.Call(new Variant[] { Variant.From(dx), Variant.From(dy) });
            return result.ToString();
        }
        Gateway.RegisterCustomFunction(Wrapper, skillName, nativeFunction.Method, description, p_list);
    }

    /** <summary>Appends text to the AI's history.</summary> */
    public void AppendHistory(string history)
    {
        Gateway.InternalContext["history"] += history;
    }

    /** <summary>Adds a bit of information to the AI's memory.</summary> */
    public void AddMemory(string id, string text)
    {
        Gateway.kernel.Memory.SaveInformationAsync("memory", id: id, text: text);
    }

    /** <summary>Adds a reference for the AI.</summary> */
    public void AddReference(string content, string description, string id, string sourceName)
    {
        Gateway.kernel.Memory.SaveReferenceAsync(
            collection: "memory",
            text: content,
            description: description,
            externalId: id,
            externalSourceName: sourceName
        );
    }

    /** <summary>Generates an embedding based on the input content.</summary> */
    public async Task<IList<Microsoft.SemanticKernel.AI.Embeddings.Embedding<float>>> GenerateEmbedding(string content)
    {
        return await Gateway.embeddingGenerator.GenerateEmbeddingsAsync(new List<string> { content });
    }

    /** <summary>Returns the accumulated history.</summary> */
    public string GetHistory()
    {
        return Gateway.InternalContext["history"];
    }

    /** <summary>Requests a decision for a plan and runs it.</summary> */
    public async void ProcessThought(string want, Godot.Collections.Dictionary world_state)
    {
        Plan plan = await RequestDecision(want, world_state);
        EmitSignal(SignalName.PlanGenerated);

        SKContext planResult = await plan.InvokeAsync();
        PlanExecutionWasSuccessful = planResult.ErrorOccurred;

        if (planResult.ErrorOccurred)
        {
            /** We can also try running the plan again! */
            GD.PushError(planResult.LastErrorDescription);
        }

        EmitSignal(SignalName.PlanFinished, planResult.Result);
    }

    /** <summary>Requests a decision from the AI brain.</summary> */
    public async Task<Plan> RequestDecision(string context, Godot.Collections.Dictionary world_state, bool again = false, string why = "")
    {
        if (again)
        {
            Gateway.InternalContext["history"] += "\n\n" + "(Try answering again because " + why + ".)" + "\n\n";
        }
        return await Gateway.RequestDecision(context, world_state);
    }
}