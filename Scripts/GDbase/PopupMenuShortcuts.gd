extends AutoPopupMenu
class_name PopupMenuShortcuts

@export var shortcuts : Array[ShortcutWithID]

# Called when the node enters the scene tree for the first time.
func _ready():
	for shortcut in shortcuts:
		set_item_shortcut(get_item_index(shortcut.id), shortcut.value)

