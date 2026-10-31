using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Google.Protobuf;
using SnakeNetwork.Protocol;

public class GameRunner
{
    //配置与成员
    private readonly GameConfig Config;
    private readonly SkinConfig SkinConfig;

    //private Game game;
    private readonly Renderer renderer;
    //游戏状况
    private bool _isRunning;
    public enum GameMode
    {
        SinglePlayer,
        LocalVersus,
        OnlineVersus
    }

    //计时器
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();
    private readonly Stopwatch? _snapshotClock;

    //net
    private Task? _snapshotPublishTask;
    private readonly MqttTransport? _network;
    private int _intervalMs = 100;
    private ulong _sequence = 0;
    private string? _publishError;


    //Constructor
    public GameRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport? network = null)
    {
        Config = config;
        SkinConfig = skinConfig;
        _network = network;
        renderer = new(config);

        if (_network != null)
            _snapshotClock = Stopwatch.StartNew();

        _isRunning = true;
    }

    //键盘输入接口 （to game）
    private void HandlePlayingInput(Game game)
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            _isRunning = false;
            return;
        }
        else if (key == ConsoleKey.Spacebar)
        {
            game.TogglePause();
        }

        if (game.State != Game.GameState.Paused)
        {
            //判断第二套按键的绑定对象
            int secondBindingPlayerId;
            if (game.gameMode == GameMode.SinglePlayer || _network != null)
                secondBindingPlayerId = 1;
            else
                secondBindingPlayerId = 2;

            //按键一
            Snake.Direction? newDirection1 = key switch
            {
                ConsoleKey.UpArrow => Snake.Direction.Up,
                ConsoleKey.DownArrow => Snake.Direction.Down,
                ConsoleKey.LeftArrow => Snake.Direction.Left,
                ConsoleKey.RightArrow => Snake.Direction.Right,
                _ => null
            };
            if (newDirection1 is { } direction1)
                game.ToggleDirection(01, direction1);

            if (key == ConsoleKey.M)
                game.ToggleBoost(01);


            //按键二
            Snake.Direction? newDirection2 = key switch
            {
                ConsoleKey.W => Snake.Direction.Up,
                ConsoleKey.S => Snake.Direction.Down,
                ConsoleKey.A => Snake.Direction.Left,
                ConsoleKey.D => Snake.Direction.Right,
                _ => null
            };

            if (newDirection2 is { } direction2)
                game.ToggleDirection(secondBindingPlayerId, direction2);

            if (key == ConsoleKey.Q)
                game.ToggleBoost(secondBindingPlayerId);
        }
    }

    private void HandleRemoteInput(Game game)
    {
        if (_network is null)
            return;
        while (_network.TryReceive(out byte[]? received) && received is not null)
        {
            PlayerInput input = PlayerInput.Parser.ParseFrom(received);
            RemoteInputProcessor.Apply(game, input);
        }
    }

    private GameMode? ChooseGameMode()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
            {
                _isRunning = false;
                return null;
            }
            if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
                return GameMode.SinglePlayer;
            if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
                return GameMode.LocalVersus;
        }
    }

    private bool HandleGameOverInput()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key is ConsoleKey.Enter)
                return true;
            if (key is ConsoleKey.Escape)
                return false;
        }
    }

    private async Task PublisherWorldAsync(Game game)
    {
        if (_network is null)
            return;

        var worldSnapshot = WorldSnapshotFactory.FromGame(Config, game, _sequence);
        byte[] worldData = worldSnapshot.ToByteArray();
        await _network.PublishAsync("snake/world", worldData);
    }

    private void UpdateWorldBroadcast(Game game)
    {

        if (_snapshotPublishTask is { IsFaulted: true } failedTask)
        {
            _publishError ??=
                failedTask.Exception?.GetBaseException().Message
                ?? "未知的 MQTT 发布错误";
        }
        else if (_snapshotPublishTask is { IsCanceled: true })
        {
            _publishError ??= "MQTT 发布任务被取消";
        }
        else if (_snapshotPublishTask is { IsCompletedSuccessfully: true })
        {
            _publishError = null;
        }

        _intervalMs = _publishError is null
            ? 100
            : 1000;

        if (_snapshotClock?.ElapsedMilliseconds >= _intervalMs)
        {
            if (_snapshotPublishTask is null || _snapshotPublishTask.IsCompleted)
            {
                _sequence++;
                _snapshotPublishTask = PublisherWorldAsync(game);
                _snapshotClock.Restart();
            }
        }


    }


    //运行游戏
    public void Rungame()
    {
        while (_isRunning)
        {
            //模式选择
            renderer.RenderMenu();
            GameMode? gameMode = ChooseGameMode();
            if (gameMode is not GameMode selectedMode)
                return;

            var game = new Game(Config, SkinConfig, selectedMode);
            //游戏循环
            while (game.State != Game.GameState.GameOver && _isRunning)
            {
                HandlePlayingInput(game);
                HandleRemoteInput(game);
                game.UpdateGameIfdue();
                UpdateWorldBroadcast(game);

                if (game.State == Game.GameState.GameOver)
                {
                    renderer.RenderFrame(game);
                    continue;
                }

                if (_renderClock.ElapsedMilliseconds >= 40)
                {
                    renderer.RenderFrame(game);
                    _renderClock.Restart();
                }

                Thread.Sleep(1);
            }

            //正常结束选择
            if (_isRunning && HandleGameOverInput() == true)
            {
                continue;
            }
            else
                break;

        }
    }

}