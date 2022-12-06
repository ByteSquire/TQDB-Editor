extends PopupMenuShortcuts

signal undo
signal redo
signal copy
signal paste
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
			copy.emit()
		4:
			paste.emit()
		6:
			edit_entry.emit()
		8:
			find.emit()
		10:
			set_default_width.emit()
	
