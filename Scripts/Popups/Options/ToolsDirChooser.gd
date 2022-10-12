extends FileDialog


func _on_tools_dir_choose_pressed():
	var current = Config.get("ToolsDir")
	current_path = current
	
	popup_centered_ratio(.5)
