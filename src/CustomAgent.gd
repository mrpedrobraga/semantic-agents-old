@tool
extends Node3D
class_name CustomAgent

const TILE_SIZE = Vector3(1, 1, 1)
@export var WORLD_POS = Vector3(0, 0, 0):
	set(v):
		WORLD_POS = v
		_update_position(v)

@export_node_path() var agent_path : NodePath
var agent : SKAgent
@export var world : GameWorld
@export var line_edit : LineEdit

var is_ready = false
func _ready():
	is_ready = true
	if Engine.is_editor_hint():
		return
	agent = get_node(agent_path)

	if !agent.IsReady:
		await agent.BecameReady;
	
	_register_commands()

	agent.ProcessThought("Agent wants to take a step.", {"position": position})

func _register_commands():
	agent.RegisterFunction(
		step,
		"Movement",
		"Takes a step in a cartesian direction.",
		[
			{"name": "dx", "description": "Delta X"},
			{"name": "dy", "description": "Delta Y"}
		]
	)

func step(dx: String, dy: String) -> String:
	var d = Vector2(float(dx), float(dy))
	WORLD_POS += Vector3(d.x, 0, d.y)
	print('moving ', dx, ', ', dy)
	return 'true'

func _update_position(new_position: Vector3):
	if !is_ready:
		return
	var p_tween = create_tween()
	p_tween.tween_property(self, "position", new_position * TILE_SIZE, 0.2)\
	.set_ease(Tween.EASE_OUT)\
	.set_trans(Tween.TRANS_QUAD)

	$"model".look_at_from_position(position, new_position)

	await p_tween.finished
