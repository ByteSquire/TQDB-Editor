extends PopupMenu

signal set_verify_option(option)


func _on_verify_options_index_pressed(index):
	for i in item_count:
		set_item_checked(i, index == i)
	set_verify_option.emit(index)
