namespace snake.Application;

using System.Diagnostics;
using Google.Protobuf;
using SnakeNetwork.Protocol;
using snake.Network;
using snake.Rendering;
using snake.Rendering.Skin;
using snake.Views;
using snake.Views.Guest;
using snake.Config;


public class GuestRunner
{
    //游戏状态（提供退出）
    private bool _isRunning = true;
    //配置与渲染环境
    private readonly Renderer renderer;

    //计时器
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();
    private readonly Stopwatch _snapshotUpdateClock = Stopwatch.StartNew();

    //net
    private readonly MqttTransport _network;
    private ulong _sequence;
    private readonly Queue<PlayerInput> _pendingInputs = new();
    private Task? _inputPublishTask;
    private IWorldView? _currentView;


    //Constructor
    public GuestRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport network
    )
    {
        _network = network;
        renderer = new(config, skinConfig);
    }


    private void HandlePlayerInput()
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            _isRunning = false;
            return;
        }

        bool useBoost = false;
        if (key is ConsoleKey.Q or ConsoleKey.M)
        {
            useBoost = true;
        }

        Direction inputDirection = key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => Direction.Up,
            ConsoleKey.DownArrow or ConsoleKey.S => Direction.Down,
            ConsoleKey.LeftArrow or ConsoleKey.A => Direction.Left,
            ConsoleKey.RightArrow or ConsoleKey.D => Direction.Right,
            _ => Direction.Unspecified
        };

        if (inputDirection == Direction.Unspecified && !useBoost)
            return;//属于优化代码，按了别的按键不会改变游戏状态，那就不发了。和不接受到键盘输入是一个道理

        var input = new PlayerInput()
        {
            PlayerId = 2,
            Direction = inputDirection,
            Boost = useBoost
        };
        _pendingInputs.Enqueue(input);
    }

    private void UpdateInputPublishing()
    {
        if (_inputPublishTask is { IsCompleted: false })
            return;

        if (_inputPublishTask is { IsFaulted: true })
            throw new InvalidOperationException(
            "玩家输入发布失败",
            _inputPublishTask.Exception);

        if (_inputPublishTask is { IsCanceled: true })
            throw new OperationCanceledException(
                "玩家输入发布被取消");

        if (_pendingInputs.Count == 0)
            return;

        PlayerInput nextInput = _pendingInputs.Dequeue();

        byte[] _inputPublish = nextInput.ToByteArray();
        _inputPublishTask = _network.PublishAsync("snake/input", _inputPublish);
    }


    private WorldSnapshot? ReceiveSnapshot()
    {
        if (!_network.TryReceive(out byte[]? received) || received is null)
            return null;

        return WorldSnapshot.Parser.ParseFrom(received);
    }

    private void UpdateWorldView()
    {
        while (ReceiveSnapshot() is { } snapshot)
        {
            if (_currentView is null || snapshot.Sequence > _sequence)
            {
                _sequence = snapshot.Sequence;
                _currentView = new GuestWorldView(snapshot);
            }
        }
    }



    //API
    public async Task RunGuestClient()
    {
        while (_isRunning)
        {
            HandlePlayerInput();
            UpdateInputPublishing();

            if (_snapshotUpdateClock.ElapsedMilliseconds >= 5)
            {
                UpdateWorldView();
                _snapshotUpdateClock.Restart();
            }

            if (_renderClock.ElapsedMilliseconds >= 40 && _currentView is { })
            {
                renderer.RenderFrame(_currentView);
                _renderClock.Restart();
            }

            Thread.Sleep(1);
        }

        if (_inputPublishTask is not null)
            await _inputPublishTask;
    }


}