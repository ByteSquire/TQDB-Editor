extends Button

@export var undefined_state : Texture2D
@export var sorted_state : Texture2D
@export var reversed_state : Texture2D

var sorted : bool = false

# Called when the node enters the scene tree for the first time.
func _ready():
	set_button_icon(undefined_state)


func _pressed():
	if sorted:
		set_button_icon(reversed_state)
		sorted = false
	else:
		set_button_icon(sorted_state)
		sorted = true
