extends Control

var col1Width : int
var col2Width : int

@export var col1 : Label
@export var col2 : Label

# Called when the node enters the scene tree for the first time.
func _ready():
	_on_header_1_dragged(col1.size.x)
	_on_header_2_dragged(col2.size.x)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_header_1_dragged(offset):
	col1Width = offset + 5
	for child in get_children():
		_on_v_box_container_child_entered_tree(child)


func _on_header_2_dragged(offset):
	col2Width = offset + 5
	for child in get_children():
		_on_v_box_container_child_entered_tree(child)


func _on_v_box_container_child_entered_tree(node):
	var bContainer = node as HBoxContainer
	if !bContainer:
		return
	
	var numChildren = bContainer.get_child_count()
	if numChildren > 0:
		var child1 = bContainer.get_child(0) as Control
		child1.clip_contents = true
		child1.set_custom_minimum_size(Vector2i(col1Width, 0))
	
	if numChildren > 1:
		var child2 = bContainer.get_child(1) as Control
		child2.clip_contents = true
		child2.set_custom_minimum_size(Vector2i(col2Width, 0))
