extends Container

var _cell_height : int
@export var cell_height : int = 10:
	get:
		return cell_height
	set(value):
		if value != _cell_height:
			_cell_height = value
			updated_cell_height()

func _notification(what):
	if what == NOTIFICATION_SORT_CHILDREN:
		var offset : int = 0
		# Must re-sort the children
		for c in get_children():
			# Fit to own size
			fit_child_in_rect( c, Rect2( Vector2(0, offset), Vector2(size.x, _cell_height) ) )
			offset += _cell_height

func updated_cell_height():
	# Some setting changed, ask for children re-sort
	queue_sort()
	
