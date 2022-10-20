extends PopupMenu

signal save
signal save_as
signal set_template
signal exit


func _on_id_pressed(id):
	match(id):
		3:
			save.emit()
		4:
			save_as.emit()
		6:
			set_template.emit()
		8:
			exit.emit()
	
