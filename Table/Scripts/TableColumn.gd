extends Container

var _cell_height : int
var _separator_height : int

@export var separator_height : int = 4:
	get:
		return _separator_height
	set(value):
		if value != _separator_height:
			_separator_height = value
			queue_sort()

@export var cell_height : int = 10:
	get:
		return _cell_height
	set(value):
		if value != _cell_height:
			_cell_height = value
			queue_sort()


func _notification(what):
	if what == NOTIFICATION_SORT_CHILDREN:
		var offset : int = cell_height / 4
		var i = 0
		# Must re-sort the children
		for c in get_children():
			# Fit to own size
			var height = cell_height
			if i % 2 == 1:
				height = separator_height
			fit_child_in_rect( c, Rect2( Vector2(0, offset), Vector2(size.x, height) ) )
			offset += height
			i += 1
	

func _get_minimum_size():
	var childCount = get_child_count()
	var numCells = childCount / 2 + 1
	var numSeparators = numCells - 1
	var combinedCellHeight = numCells * cell_height
	var combinedSeparatorHeight = numSeparators * separator_height
	var offsetTopAndBottom = cell_height
	return Vector2i(0, combinedCellHeight + combinedSeparatorHeight + offsetTopAndBottom)
