using System.Diagnostics;
using Google.Protobuf;
using SnakeNetwork.Protocol;
using snake.Core;
using GameDirection = snake.Core.Primitives.Direction;
using snake.Core.GameTypes;
using snake.Rendering;
using snake.Rendering.Skin;
using CoreGameState = snake.Core.GameTypes.GameState;
using snake.Input;


public class GameRunner
{
    //Config and Member
    private readonly GameConfig Config;
    private readonly Renderer renderer;

    //游戏状况
    private bool _isRunning;

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
        _network = network;
        renderer = new(config, skinConfig);

        if (_network != null)
            _snapshotClock = Stopwatch.StartNew();

        _isRunning = true;
    }

    //键盘输入接口 （to game）
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

        if (game.GameState != CoreGameState.Paused)
        {
            //判断第二套按键的绑定对象（根据创建游戏的gamemode）
            int secondBindingPlayerId;
            if (game.Mode == GameMode.SinglePlayer || _network != null)
                secondBindingPlayerId = 1;
            else
                secondBindingPlayerId = 2;

            //按键一
            GameDirection? newDirection1 = key.ToDirectionFromArrowKeys();
            if (newDirection1 is { } direction1)
                game.ToggleDirection(01, direction1);

            if (key == ConsoleKey.M)
                game.ToggleBoost(01);

/////////////////////////键位待修复/////////////////////
            //按键二
            GameDirection? newDirection2 = key.ToDirectionFromWasdKeys();
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


    private async Task PublisherSnapshotAsync(Game game)
    {
        if (_network is null)
            return;

        var worldSnapshot = WorldSnapshotFactory.FromGame(Config, game, _sequence);
        byte[] worldData = worldSnapshot.ToByteArray();
        await _network.PublishAsync("snake/world", worldData);
    }

    private void UpdateSnapshotBroadcast(Game game)
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
                _snapshotPublishTask = PublisherSnapshotAsync(game);
                _snapshotClock.Restart();
            }
        }
    }

    private async Task PublishFinalSnapshotAsync(Game game)
    {
        if (_network is null)
            return;

        if (_snapshotPublishTask is not null)
        {
            try
            {
                await _snapshotPublishTask;
            }
            catch (Exception ex)
            {
                _publishError = ex.GetBaseException().Message;
            }
        }

        _sequence++;
        await PublisherSnapshotAsync(game);
    }


    //运行游戏
    public async Task RungameAsync()
    {
        while (_isRunning)
        {
            //环节一：模式选择·创建游戏
            if (_network is null)
                renderer.RenderMenu();

            GameMode? gameMode = _network is null
                ? SelectGameMode()
                : GameMode.OnlineVersus;

            if (gameMode is not GameMode selectedMode)
            {
                _isRunning = false;
                break;
            }

            var game = new Game(Config, selectedMode);

            //环节二：游戏循环
            while (game.GameState != CoreGameState.GameOver && _isRunning)
            {
                HandlePlayingInput(game);
                HandleRemoteInput(game);
                game.UpdateGameIfdue();

                //保证最后一帧能正常渲染与发布
                if (game.GameState == CoreGameState.GameOver)
                {
                    renderer.RenderFrame(game);
                    await PublishFinalSnapshotAsync(game);
                    break;
                }
                UpdateSnapshotBroadcast(game);

                if (_renderClock.ElapsedMilliseconds >= 40)
                {
                    renderer.RenderFrame(game);
                    _renderClock.Restart();
                }

                Thread.Sleep(1);
            }

            //环节三：游戏结束交互
            if (_isRunning && HandleGameOverInput() == true)
                continue;
            else
                _isRunning = false;
        }
    }

}