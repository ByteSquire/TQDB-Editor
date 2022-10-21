extends PopupMenu

signal save
signal save_as
signal set_template
signal exit


var _save = preload("res://Shortcuts/save.tres")

func _ready():
	set_item_shortcut(get_item_index(3), _save)
	


func _on_id_pressed(id):
	match(id):
		3:
			save.emit()
		4:
			save_as.emit()
		6:
			set_template.emit()
		8:
			exit.emit()
	
