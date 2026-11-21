#nullable enable

using System;
using Godot;
using R3;
using Snake.Config;
using Snake.Core;
using Snake.Core.GameTypes;

namespace Snake.Godot;

public partial class GameController : Node
{

	private GameConfig? _config;
	private Game? _game;


	[Signal]
	public delegate void PhaseChangeEventHandler(string phase);

	//游戏结束需要主动通知ui
	private readonly ReactiveProperty<AppPhase> _phase = new(AppPhase.Menu);
	private IDisposable _phaseSubscription = null!;

	public override void _Ready()
	{
		//使用signal主动通知ui_controllor
		_phaseSubscription = _phase
			.Select(phase => phase.ToString())
			.Subscribe(phaseString =>
			{
				EmitSignal(SignalName.PhaseChange, phaseString);
			});

		EnterMenu();

		try
		{
			_config = GodotConfigLoader.Load(
				"res://Config/configClassic.json"
			);
		}
		catch (Exception ex)
		{
			GD.PushError($"配置加载失败：{ex.Message}");
			return;
		}

	}

	public override void _PhysicsProcess(double delta) //现在的写法有挺大的问题但暂且不管……
	{
		if (_phase.Value != AppPhase.InGame || _game is null)
			return;

		Game game = _game;

		if (game.GameState == GameState.Playing)
		{
			game.UpdateGameIfdue();
		}

		if (game.GameState == GameState.GameOver)
		{
			EnterResults();
		}
	}

	// 1.根据按钮模式创建一局游戏
	public void StartGame(GameMode mode)
	{
		if (_phase.Value != AppPhase.Menu || _config is null)
			return;

		_game = new Game(_config, mode);

		ChangePhase(AppPhase.InGame);
	}

	//2.ui交互接口：退出、
	public void ExitGame()
	{
		GetTree().Quit();
	}

	public void EnterMenu()
	{
		_game = null;
		ChangePhase(AppPhase.Menu);
	}

	//更改Phase的回调函数
	private void ChangePhase(AppPhase phase) =>
		_phase.Value = phase;

	private void EnterResults()
	{
		ChangePhase(AppPhase.Results);
	}

}
