extends PopupMenu

signal undo
signal redo
signal edit_entry
signal find
signal set_default_width

func _on_id_pressed(id):
	match(id):
		0:
			undo.emit()
		1:
			redo.emit()
		3:
			edit_entry.emit()
		5:
			find.emit()
		7:
			set_default_width.emit()
	
