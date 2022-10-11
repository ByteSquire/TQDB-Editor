extends PopupMenu

@export var files_node : NodePath
@export var file_dialog : FileDialog

#signal set_working_folder(folder_path)
signal rename()
signal exit()


func _on_id_pressed(id):
	match(id):
		0:
			var dia = file_dialog
			dia.size = Vector2(0,0)
			var current = Config.get("WorkingDir")
			if current:
				dia.set_current_path(current)
			dia.dir_selected.connect(_on_working_dir_selected)
			dia.popup_centered()
		1:
			rename.emit()
		3:
			exit.emit()
			get_tree().call_deferred("quit")


func _on_working_dir_selected(path):
	print("setting working dir to: " + path)
	Config.set("WorkingDir", path)
