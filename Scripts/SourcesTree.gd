extends Tree


# Called when the node enters the scene tree for the first time.
func _ready():
	var tree = self
	var root = tree.create_item()
	root.set_text(0, "Test root")
#	tree.set_hide_root(true)
	var child1 = tree.create_item(root)
	child1.set_text(0, "Test Child")
	var subchild1 = tree.create_item(child1)
	subchild1.set_text(0, "Test Subchild")
