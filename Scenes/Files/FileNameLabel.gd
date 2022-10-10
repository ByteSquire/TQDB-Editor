extends Label

signal label_clicked(sender)
signal label_selected(sender)
var isSelected : bool
var parentNode : Node

func _ready():
	isSelected = false
	parentNode.connect("DeselectLabel", _on_deselect)


func _gui_input(event):
	if event is InputEventMouseButton:
		if not (event as InputEventMouseButton).pressed and not isSelected:
			set_theme_type_variation("SelectedLabel")
			emit_signal("label_selected", self)
			isSelected = true
		if (event as InputEventMouseButton).double_click:
			emit_signal("label_clicked", self)


func _on_deselect(selected):
	if not isSelected:
		return
	
	print(self.text)
	print(selected.text)
	if self != selected:
		print("unequal")
		isSelected = false
		set_theme_type_variation("")
