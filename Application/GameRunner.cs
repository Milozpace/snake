namespace snake.Application;

using System.Diagnostics;
using Google.Protobuf;
using SnakeNetwork.Protocol;
using snake.Core;
using GameDirection = Core.Primitives.Direction;
using snake.Core.GameTypes;
using snake.Rendering;
using snake.Rendering.Skin;
using CoreGameState = Core.GameTypes.GameState;
using snake.Input;
using snake.Network;
using snake.Config;


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

            //按键二
            GameDirection? newDirection2 = key.ToDirectionFromWasdKeys();
            if (newDirection2 is { } direction2)
                game.ToggleDirection(secondBindingPlayerId, direction2);

            if (key == ConsoleKey.Q)
                game.ToggleBoost(secondBindingPlayerId);
        }
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

    //广播📢
    private async Task PublisherSnapshotAsync(Game game)//1.获取数据 2.序列化 3.发布 （其实就是游戏的一个发布模块）
    {
        if (_network is null)
            return;

        var worldSnapshot = WorldSnapshotFactory.ToWorldSnapshot(Config, game, _sequence);
        //proto类的一个方法，根据game和config获取需要传输的数据
        byte[] worldData = worldSnapshot.ToByteArray();
        await _network.PublishAsync("snake/world", worldData);
    }

    private void UpdateSnapshotBroadcast(Game game)//核心处理控制发布任务的调用
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
            if (_snapshotPublishTask is null || _snapshotPublishTask.IsCompleted) //根据对应的任务状态决定要不要发送
            {
                _sequence++;
                _snapshotPublishTask = PublisherSnapshotAsync(game);
                _snapshotClock.Restart();
            }
        }
    }

    private async Task PublishFinalSnapshotAsync(Game game) //最后一帧的广播功能
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

            //环节二：游戏循环（网络处理、游戏处理、渲染调用）
            while (game.GameState != CoreGameState.GameOver && _isRunning)
            {
                HandlePlayingInput(game);
                HandleRemoteInput(game);//网络调用1
                game.UpdateGameIfdue();

                //保证最后一帧能正常渲染与发布
                if (game.GameState == CoreGameState.GameOver)
                {
                    renderer.RenderFrame(game);
                    await PublishFinalSnapshotAsync(game); //调用二
                    break;
                }
                UpdateSnapshotBroadcast(game);//调用三

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