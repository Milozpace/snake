extends Control

@export var menu : Control;
@export var playing : Control;
@export var results : Control;

@export var controller : Node;


func _ready() -> void:
	if (menu == null or playing == null or results == null):
		push_error("UI 节点未绑定完整，请检查编辑器中的导出变量！");
		return;
	
	if controller:
		controller.PhaseChange.connect(_on_phase_changed)
	else:
		push_error("未绑定 controller! ")
		return;

	_on_phase_changed("Menu")
	

#接收通知，控制页面显示。
func _on_phase_changed(phase: String) -> void:
	menu.visible = phase == "Menu"
	playing.visible = phase == "InGame"
	results.visible = phase == "Results"
	
