extends RichTextLabel

signal activated(label : RichTextLabel)

func _gui_input(event):
	if event is InputEventMouseButton and event.is_pressed() and event.double_click and event.button_index==1:
		activated.emit(self)
	if event is InputEventKey and event.is_pressed() and event.keycode == KEY_ENTER:
		activated.emit(self)
	
