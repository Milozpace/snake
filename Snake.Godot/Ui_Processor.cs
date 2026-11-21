using Godot;
using Snake.Core.GameTypes;

namespace Snake.Godot;

//按钮信号已通过编辑器connect

public partial class Ui_Processor : Node
{
	[Export] private GameController _gameController;

	public override void _Ready()
	{
		if (_gameController == null)
		{
			GD.PrintErr("错误: GameController 未在编辑器中绑定！");
			return;
		}
	}

	//开始游戏的按钮信号
	private void OnSingleButtonPressed() =>
		_gameController.StartGame(GameMode.SinglePlayer);

	private void OnVersusButtonPressed() =>
		_gameController.StartGame(GameMode.LocalVersus);

	//结束游戏的按钮信号
	private void OnReplayButtonPressed() =>
		_gameController.EnterMenu();
	
	private void OnExitButtonPressed() =>
		_gameController.ExitGame();
}
