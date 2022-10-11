extends PopupMenu

signal help
signal about

signal install_templates
signal install_tutorials


func _on_id_pressed(id):
	match(id):
		0:
			help.emit()
		1:
			about.emit()
		3:
			install_templates.emit()
		4:
			install_tutorials.emit()
