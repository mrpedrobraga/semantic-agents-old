extends Node3D
class_name GameWorld

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

var internal_repr = {
	"entities": {
		"apple_3$Df": Vector2(10, 10),
		"apple_54$a": Vector2(0, 3)
	}
}

func _repr():
	return internal_repr

func interact(agent, what:String):
	if internal_repr.entities.has(what):
		internal_repr.entities.erase(what)
		return true
	return false
