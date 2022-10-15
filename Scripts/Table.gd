extends Control


@export var columns : Array[String]

@export var header_column : PackedScene
@export var column_button : PackedScene
@export var column : PackedScene

var _columns : Array[VBoxContainer]

var _content

# Called when the node enters the scene tree for the first time.
func _ready():
	_content = get_child(0)
	if columns != null and columns.size() > 0:
		var node = _content
		var node1 = node.get_child(0)
		for i in columns.size():
			var header = header_column.instantiate()
			(header.get_child(0) as Button).text = columns[i]
			header.get_child(0).name = "Column" + str(i + 1)
			
			var _column = column.instantiate()
			_column.get_child(0).name = "Column" + str(i + 1)
#			for j in 500:
#				var test_label = RichTextLabel.new()
#				test_label.text = "Test but it's quite long to see how that behaves but I don't know if this is reasonable it feels like SHIT"
##				test_label.clip_text = true
#				test_label.autowrap_mode = 0
#				test_label.fit_content_height = true
#				test_label.custom_minimum_size = Vector2(100,0)
#				_column.get_child(0).add_child(test_label)
			
			header.set("other", _column)
			_column.set("other", header)
			node.add_child(header)
			node = header
			node1.add_child(_column)
			node1 = _column
		
		(node.get_parent() as HSplitContainer).dragged.connect(_on_last_split_dragged)
		
		_content.move_child(_content.get_child(0), 1)
		_content.custom_minimum_size = _content.size
	


var lastDrag = 0

func _on_last_split_dragged(x):
	_content.custom_minimum_size += Vector2i(x - lastDrag,0)
	lastDrag = x
	


func add_row(values : Array[Control]):
	if not values is Array:
		printerr("You must pass an array containing the control elements inside the row")
		return
	
	if not values.size() == columns.size():
		printerr("You must pass an array that contains a value for every column!")
		return
	
	for i in values.size():
		values[i].custom_minimum_size = Vector2(100,0)
		_columns[i].add_child(values[i])
	
	return _columns[0].get_child_count()
	


func get_cell_at(cell_position : Vector2i):
	return _columns[cell_position.x].get_child(cell_position.y)
	


func get_row(index : int):
	var ret = []
	
	for _column in _columns:
		ret += [_column.get_child(index)]
	
	return ret
	
