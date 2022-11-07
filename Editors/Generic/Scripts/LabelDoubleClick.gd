extends Label

signal activated(label : Label)


func _gui_input(event):
	if event is InputEventMouseButton and event.is_pressed() and event.double_click and event.button_index==1:
		activate()
	if event is InputEventKey and event.is_pressed() and event.keycode == KEY_ENTER:
		activate()
	

func activate():
	activated.emit(self)


func _on_focus_entered():
	theme_type_variation = "SelectedLabel"


func _on_focus_exited():
	theme_type_variation = "LabelWithBorder"
