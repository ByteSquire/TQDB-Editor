extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_directories_tab_changed(tab):
	var children = get_children()
	for i in get_child_count():
		children[i].visible = i == tab
