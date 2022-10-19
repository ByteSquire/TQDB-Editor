extends HSplitContainer

@export var other : HSplitContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	if not other:
		return
	other.connect("dragged", _on_other_dragged)


func _on_other_dragged(offset):
	if split_offset == offset:
		return
	split_offset = offset
	dragged.emit(offset)


func set_synced_offset(offset):
	split_offset = offset
	other.split_offset = offset
	dragged.emit(offset)
	other.dragged.emit(offset)
