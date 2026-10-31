using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using SnakeNetwork.Protocol;

//shikoto wa:
//组织guest的接收世界数据 -> 发布input -> 渲染画面 的小型程序

public class GuestRunner
{
    //配置与渲染环境
    private readonly GameConfig Config;
    private readonly SkinConfig SkinConfig;
    private readonly Renderer renderer;

    //计时器
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();
    private readonly Stopwatch _snapshotUpdateClock = Stopwatch.StartNew();

    //net
    private readonly MqttTransport _network;
    private ulong _sequence;
    private readonly Queue<PlayerInput> _pendingInputs = new();
    private Task? _inputPublishTask;
    private IGameView? _currentView;


    //Constructor
    public GuestRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport network
    )
    {
        Config = config;
        SkinConfig = skinConfig;
        _network = network;
        renderer = new(config, skinConfig);
    }


    private void HandlePlayerInput()
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        bool useBoost = false;
        if (key is ConsoleKey.Q or ConsoleKey.M)
        {
            useBoost = true;
        }

        Direction inputDirection = key switch
        {
            ConsoleKey.UpArrow => Direction.Up,
            ConsoleKey.DownArrow => Direction.Down,
            ConsoleKey.LeftArrow => Direction.Left,
            ConsoleKey.RightArrow => Direction.Right,
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


    //！！非常的干货
    //？？管理_currentView，保持最新的状态
    private void UpdateWorldView()
    {
        while (ReceiveSnapshot() is { } snapshot)
        {
            if (_currentView is null || snapshot.Sequence > _sequence)//意义不明但暂时保留……
            {
                _sequence = snapshot.Sequence;
                _currentView = new GuestWorldView(snapshot);
            }
        }
    }



    //API
    public void RunGuestClient()
    {
        while (true)
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

    }


}