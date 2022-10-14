extends ScrollContainer

var other_scrolls : Array[ScrollBar]


# Called when the node enters the scene tree for the first time.
func _ready():
	get_v_scroll_bar().value_changed.connect(_on_scrolled)

func _on_scrolled(value):
	for scroll in other_scrolls:
		scroll.value = value
