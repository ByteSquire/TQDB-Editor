extends PopupMenu

signal import_record

signal check_records
signal move_records
signal show_archive_stats

signal stop


func _on_id_pressed(id):
	match(id):
		0:
			import_record.emit()
		2:
			check_records.emit()
		3:
			move_records.emit()
		4:
			show_archive_stats.emit()
		6:
			stop.emit()
	

func _on_build_mod_toggle_build():
	var index = get_item_index(6)
	set_item_disabled(index, not is_item_disabled(index))
	
