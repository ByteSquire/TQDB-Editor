extends AutoPopupMenu

signal build
signal stop

signal verify_build


func _on_id_pressed(id):
	match(id):
		0:
			build.emit()
		1:
			stop.emit()
		3:
			verify_build.emit()
