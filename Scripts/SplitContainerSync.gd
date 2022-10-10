extends HSplitContainer

@export var other : HSplitContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	if not other:
		return
	other.connect("dragged", _on_other_dragged)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _on_other_dragged(offset):
	split_offset = offset
