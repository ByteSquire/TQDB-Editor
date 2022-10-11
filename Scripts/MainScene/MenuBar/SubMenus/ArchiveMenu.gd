extends PopupMenu

signal build
signal compact

signal show_archive_stats


func _on_id_pressed(id):
	match(id):
		0:
			build.emit()
		1:
			compact.emit()
		3:
			show_archive_stats.emit()
