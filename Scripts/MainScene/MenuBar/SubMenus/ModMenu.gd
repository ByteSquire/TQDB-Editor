extends AutoPopupMenu

signal new
signal delete


func _on_id_pressed(id):
	match(id):
		0:
			new.emit()
		1:
			delete.emit()
