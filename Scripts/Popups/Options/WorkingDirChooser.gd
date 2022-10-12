extends FileDialog


func _on_working_dir_choose_pressed():
	var current = Config.get("WorkingDir")
	current_path = current
	
	popup_centered_ratio(.5)
