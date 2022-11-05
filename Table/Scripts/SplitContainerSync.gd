extends HSplitContainer

@export var other : HSplitContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	if not other:
		return
	other.connect("dragged", _on_other_dragged)
	get_child(get_child_count(true)-1, true).gui_input.connect(_dragger_gui_input)


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


func _dragger_gui_input(event):
	if event is InputEventMouseButton and event.is_pressed() and event.double_click and event.button_index==1:
		set_synced_offset(0)
