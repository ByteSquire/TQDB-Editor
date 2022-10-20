extends PopupMenu
class_name AutoPopupMenu

# Called when the node enters the scene tree for the first time.
func _ready():
	var submenus = get_children().filter(func(x): return x is PopupMenu)
	for submenu in submenus:
		add_submenu_item(submenu.name, submenu.name)
