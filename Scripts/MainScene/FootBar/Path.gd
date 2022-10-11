extends Label

@export var configNode : Node

# Called when the node enters the scene tree for the first time.
func _ready():
	if configNode:
		set_text(configNode.call("GetModPath"))

