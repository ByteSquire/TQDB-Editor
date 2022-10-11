extends PopupMenu

signal toggle_status_bar
signal refresh


func _on_id_pressed(id):
	match(id):
		0:
			toggle_status_bar.emit()
		1:
			refresh.emit()
		
