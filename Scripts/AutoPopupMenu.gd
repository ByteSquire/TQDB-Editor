extends PopupMenu
class_name AutoPopupMenu

# Called when the node enters the scene tree for the first time.
func _ready():
	print(get_children())
	var submenus = get_children().filter(func(x): return x is PopupMenu)
	print(submenus)
	for submenu in submenus:
		add_submenu_item(submenu.name, submenu.name)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
