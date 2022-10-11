extends PopupMenu

signal undo
signal select_all

signal copy
signal paste
signal delete


func _on_id_pressed(id):
	match(id):
		0:
			undo.emit()
		1:
			select_all.emit()
		
		3:
			copy.emit()
		4:
			paste.emit()
		5:
			delete.emit()
