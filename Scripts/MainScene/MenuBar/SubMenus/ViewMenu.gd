extends PopupMenu

signal toggle_status_bar
signal refresh

var status_bar_enabled = true

func _on_id_pressed(id):
	match(id):
		0:
			status_bar_enabled = not status_bar_enabled
			set_item_checked(get_item_index(id), status_bar_enabled)
			toggle_status_bar.emit()
		1:
			refresh.emit()
		
