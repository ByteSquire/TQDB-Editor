extends PopupMenu

signal select(mod_name)


func _on_select_index_pressed(index):
	for i in item_count:
		set_item_checked(i, index == i)
	select.emit(get_item_text(index))
