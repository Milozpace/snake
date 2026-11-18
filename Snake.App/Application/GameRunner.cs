namespace snake.Application;

using System.Diagnostics;
using SnakeNetwork.Protocol;
using snake.Core;
using snake.Core.GameTypes;
using snake.Rendering;
using snake.Rendering.Skin;
using CoreGameState = Core.GameTypes.GameState;
using snake.Input;
using snake.Network;
using snake.Config;
using R3;


public class GameRunner
{
    //Config and Member
    private readonly GameConfig _config;
    private readonly Renderer _renderer;

    //计时器
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();

    //net与发布模块
    private readonly MqttTransport? _network;
    private readonly WorldSnapshotBroadcaster? _snapshotBroadcaster;

    //交互事件流
    private readonly Subject<ConsoleKey> _keyStream = new();


    //Constructor
    public GameRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport? network = null)
    {
        _config = config;
        _network = network;
        _renderer = new(config, skinConfig);

        if (_network is not null)
            _snapshotBroadcaster = new(config, _network);
    }

    private void RegisterInputSubscriptions(Game game, int wasdPlayerId)
    {
        var pauseSubscription = _keyStream
            .Where(key => key is ConsoleKey.Spacebar)
            .Subscribe(_ => game.TogglePause());

        var arrowTurnSubscription = _keyStream
            .Select(key => key.ToGameDirectionFromArrowKeys())
            .Where(direction => direction.HasValue)
            .Select(direction => direction!.Value) //空值宽恕运算符
            .Subscribe(direction => game.ToggleDirection(01, direction));

        var wasdTurnSubscription = _keyStream
            .Select(key => key.ToGameDirectionFromWasdKeys())
            .Where(direction => direction.HasValue)
            .Select(direction => direction!.Value)
            .Subscribe(direction => game.ToggleDirection(wasdPlayerId, direction));

        var arrowBoostSubscription = _keyStream
            .Where(key => key is ConsoleKey.M)
            .Subscribe(_ => game.ToggleBoost(01));

        var wasdBoostSubscription = _keyStream
            .Where(key => key is ConsoleKey.Q)
            .Subscribe(_ => game.ToggleBoost(wasdPlayerId));
    }

    private bool HandlePlayingInput()
    {
        if (!Console.KeyAvailable)
            return true;

        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.Escape)
            return false;

        _keyStream.OnNext(key);
        return true;
    }

    private void HandleRemoteInput(Game game) //1.接收input信息 2.解析 3.调用应用
    {
        if (_network is null)
            return;
        while (_network.TryReceive(out byte[]? received) && received is not null)
        {
            PlayerInput input = PlayerInput.Parser.ParseFrom(received);
            RemoteInputProcessor.ApplyRemoteInput(game, input);
        }
    }

    private static GameMode? SelectGameMode()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
                return null;

            if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
                return GameMode.SinglePlayer;
            if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
                return GameMode.LocalVersus;
        }
    }

    private static bool HandleGameOverInput()
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

    private static int BindingSecondPlayerId(GameMode mode)
    {
        return mode switch
        {
            GameMode.SinglePlayer => 1,
            GameMode.LocalVersus => 2,
            GameMode.OnlineVersus => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }


    //运行游戏
    public async Task RunGameAsync()
    {
        while (true)
        {
            //环节一：模式选择·创建游戏
            if (_network is null)
                _renderer.RenderMenu();

            GameMode? gameMode = _network is null
                ? SelectGameMode()
                : GameMode.OnlineVersus;

            if (gameMode is not GameMode selectedMode)
                return;


            var game = new Game(_config, selectedMode);
            int wasdPlayerId = BindingSecondPlayerId(selectedMode);
            RegisterInputSubscriptions(game, wasdPlayerId);

            //环节二：游戏循环（网络处理、游戏处理、渲染调用）
            while (game.GameState != CoreGameState.GameOver)
            {
                if (!HandlePlayingInput())
                    return;

                HandleRemoteInput(game);
                game.UpdateGameIfdue();

                //保证最后一帧能正常渲染与发布
                if (game.GameState == CoreGameState.GameOver)
                {
                    _renderer.RenderFrame(game.CaptureFrame());
                    if (_snapshotBroadcaster is not null)
                        await _snapshotBroadcaster.PublishFinalSnapshotAsync(game);
                    break;
                }
                _snapshotBroadcaster?.UpdateSnapshotBroadcast(game);

                if (_renderClock.ElapsedMilliseconds >= 40)
                {
                    _renderer.RenderFrame(game.CaptureFrame());
                    _renderClock.Restart();
                }

                Thread.Sleep(1);
            }

            //环节三：游戏结束交互
            if (HandleGameOverInput() == false)
                return;
        }
    }

}
