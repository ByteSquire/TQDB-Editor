extends Control


func _on_directories_tab_changed(tab):
	var children = get_children()
	for i in get_child_count():
		children[i].visible = i == tab
