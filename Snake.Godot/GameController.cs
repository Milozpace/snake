#nullable enable

using System;
using Godot;
using R3;
using snake.Config;
using snake.Core;
using snake.Core.GameTypes;

namespace Snake.Godot;

public partial class GameController : Node
{

	private GameConfig? _config;
	private Game? _game;

	// 引用
	[Export] private Button _singleButton = null!;
	[Export] private Button _versusButton = null!;
	[Export] private Button _replayButton = null!;
	[Export] private Button _exitButton = null!;

	[Signal]
	public delegate void PhaseChangeEventHandler(string phase);

	private readonly ReactiveProperty<AppPhase> _phase = new(AppPhase.Menu);

	private IDisposable _phaseSubscription = null!;



	public override void _Ready()
	{
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
			_singleButton.Disabled = true;
			_versusButton.Disabled = true;

			GD.PushError($"配置加载失败：{ex.Message}");
			return;
		}

		ConnectButtons();
	}

	//1. 信号连接与函数
	private void ConnectButtons()
	{
		_singleButton.Pressed += OnSinglePressed;
		_versusButton.Pressed += OnVersusPressed;
		_replayButton.Pressed += EnterMenu;
		_exitButton.Pressed += ExitGame;
	}

	private void OnSinglePressed()
	{
		StartGame(GameMode.SinglePlayer);
	}

	private void OnVersusPressed()
	{
		StartGame(GameMode.LocalVersus);
	}

	// 创建一局：由按钮回调调用。
	private void StartGame(GameMode mode)
	{
		if (_phase.Value != AppPhase.Menu || _config is null)
			return;

		_game = new Game(_config, mode);

		ChangePhase(AppPhase.InGame);
	}

	private void ExitGame()
	{
		GetTree().Quit();
	}


	// 2.持续调度：由 Godot 定期调用。
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

	// 3.界面与游戏phase管理
	private void EnterResults()
	{
		ChangePhase(AppPhase.Results);
	}

	private void EnterMenu()
	{
		_game = null;
		ChangePhase(AppPhase.Menu);
	}

	private void ChangePhase(AppPhase phase) =>
		_phase.Value = phase;



}
