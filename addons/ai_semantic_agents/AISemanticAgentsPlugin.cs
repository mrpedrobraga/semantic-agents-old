#if TOOLS
using Godot;
using System;

namespace SemanticAgent;

[Tool]
public partial class AISemanticAgentsPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		AddCustomType(
			"SKAgent",
			"Node",
			(Script)GD.Load("res://addons/ai_semantic_agents/gateway/SKAgent.cs"),
			null
		);
		AddCustomType(
			"SKGateway",
			"Resource",
			(Script)GD.Load("res://addons/ai_semantic_agents/gateway/SKGateway.cs"),
			null
		);
	}

	public override void _ExitTree()
	{
		RemoveCustomType("SKGateway");
		RemoveCustomType("SKAgent");
	}
}
#endif
