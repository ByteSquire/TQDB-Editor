extends ConfirmationDialog


func _on_options_confirmed():
	var working = get_node("VBoxContainer/GridContainer/WorkingDirText").text
	var build = get_node("VBoxContainer/GridContainer/BuildDirText").text
	var tools = get_node("VBoxContainer/GridContainer/ToolsDirText").text
	Config.set("WorkingDir", working)
	Config.set("BuildDir", build)
	Config.set("ToolsDir", tools)
