extends ItemList

@export var sort_button : Button

var sorted : bool = false 

# Called when the node enters the scene tree for the first time.
func _ready():
	empty_clicked.connect(_on_empty_clicked)
	if sort_button:
		sort_button.pressed.connect(_on_sort_pressed)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _on_empty_clicked(at_position, mouse_button_index):
	deselect_all()


func _on_sort_pressed():
	if sorted:
		print("reverse")
		sort_items_by_text()
		for i in item_count:
			print(str(0) + "-" + str(item_count - 1 - i))
			move_item(0, item_count - 1 - i)
		sorted = false
	else:
		print("sort")
		sort_items_by_text()
		sorted = true
		
