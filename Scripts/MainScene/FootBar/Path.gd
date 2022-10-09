extends Label

@export var configNode : Node

# Called when the node enters the scene tree for the first time.
func _ready():
	if configNode:
		set_text(configNode.call("GetModPath"))


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
