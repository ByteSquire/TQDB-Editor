extends PopupMenu

signal import_record(path)

signal check_records
signal move_records
signal show_archive_stats

signal stop


func _on_id_pressed(id):
	match(id):
		0:
			# open file dialog to choose record
			# or use archivetool?
			import_record.emit("test")
		2:
			check_records.emit()
		3:
			move_records.emit()
		4:
			show_archive_stats.emit()
		6:
			stop.emit()
	
