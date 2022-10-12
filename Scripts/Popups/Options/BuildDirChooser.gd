extends FileDialog


func _on_build_dir_choose_pressed():
	var current = Config.get("BuildDir")
	current_path = current
	
	popup_centered_ratio(.5)
