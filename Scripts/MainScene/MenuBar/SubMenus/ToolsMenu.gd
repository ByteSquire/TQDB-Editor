extends PopupMenu

signal extract_game_files
signal options


func _on_id_pressed(id):
	match(id):
		0:
			extract_game_files.emit()
		2:
			options.emit()
