extends PopupMenu

signal show_template
signal toggle_descriptions
signal toggle_toolbar
signal toggle_statusbar

var description : bool = true
var toolbar : bool = false
var statusbar : bool = true

func _on_id_pressed(id):
	var index = get_item_index(id)
	match(id):
		0:
			show_template.emit()
		2:
			description = not description
			set_item_checked(index, description)
			toggle_descriptions.emit()
		3:
			toolbar = not toolbar
			set_item_checked(index, toolbar)
			toggle_toolbar.emit()
		4:
			statusbar = not statusbar
			set_item_checked(index, statusbar)
			toggle_statusbar.emit()
