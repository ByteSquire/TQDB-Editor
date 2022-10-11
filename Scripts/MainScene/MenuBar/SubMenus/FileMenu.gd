extends PopupMenu

signal set_working_folder
signal rename()
signal exit()


func _on_id_pressed(id):
	match(id):
		0:
			set_working_folder.emit()
		1:
			rename.emit()
		3:
			exit.emit()
			get_tree().call_deferred("quit")
