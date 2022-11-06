extends Label


# Called when the node enters the scene tree for the first time.
func _ready():
	if Config:
		set_text(Config.call("GetModPath"))
		Config.connect("ModNameChanged", func(): set_text(Config.call("GetModPath")))
		

