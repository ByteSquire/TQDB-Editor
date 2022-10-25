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


func _on_build_mod_toggle_build():
	var index = get_item_index(1)
	set_item_disabled(index, not is_item_disabled(index))
