extends Control

@export var menu : Control;
@export var playing : Control;
@export var results : Control;

@onready var controller = get_parent();


func _ready() -> void:
	controller.PhaseChange.connect(_on_phase_changed)

	_on_phase_changed("Menu")
	

func _on_phase_changed(phase: String) -> void:
	menu.visible = phase == "Menu"
	playing.visible = phase == "InGame"
	results.visible = phase == "Results"
	
	
	
