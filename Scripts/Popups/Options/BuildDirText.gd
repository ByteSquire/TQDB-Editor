extends LineEdit


func _on_build_dir_chooser_dir_selected(dir):
	text = dir
	text_changed.emit(text)


func _on_options_about_to_popup():
	text = Config.get("BuildDir")
	text_changed.emit(text)
