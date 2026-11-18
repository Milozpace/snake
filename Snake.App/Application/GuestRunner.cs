namespace Snake.App.Application;

using System.Diagnostics;
using SnakeNetwork.Protocol;
using Snake.Network;
using Snake.Rendering;
using Snake.Rendering.Skin;
using Snake.Views;
using Snake.Config;
using Snake.App.Input;

public class GuestRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport network
    )
{
    //游戏状态
    private bool _isRunning = true;
    //配置与渲染环境
    private readonly Renderer _renderer= new(config, skinConfig);

    //计时器(处理渲染和接收事件)
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();
    private readonly Stopwatch _snapshotUpdateClock = Stopwatch.StartNew();

    //net
    private readonly MqttTransport _network = network;
    private readonly GuestInputBroadcaster _gameInputBroadcaster = new(network);
    private ulong _snapshotSequence;
    private WorldFrame? _currentFrame;


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
            useBoost = true;
        

        Direction inputDirection = key.ToProtoDirection();

        if (inputDirection == Direction.Unspecified && !useBoost)
            return;

        var input = new PlayerInput()
        {
            PlayerId = 2,
            Direction = inputDirection,
            Boost = useBoost
        };
        _gameInputBroadcaster.EnqueueInput(input);
    }

    

    private WorldSnapshot? ReceiveSnapshot()
    {
        if (!_network.TryReceive(out byte[]? received) || received is null)
            return null;

        return WorldSnapshot.Parser.ParseFrom(received);
    }

    private void UpdateWorldFrame()
    {
        while (ReceiveSnapshot() is { } snapshot)
        {
            if (_currentFrame is null || snapshot.Sequence > _snapshotSequence)
            {
                _snapshotSequence = snapshot.Sequence;
                _currentFrame = WorldSnapshotFactory.ToWorldFrame(snapshot);
            }
        }
    }

    //API
    public async Task RunGuestClient()
    {
        while (_isRunning)
        {
            //1.发布：处理输入发布（两个接口）
            HandlePlayerInput();
            _gameInputBroadcaster.UpdateInputPublishing();

            //2.接收：更新世界状态
            if (_snapshotUpdateClock.ElapsedMilliseconds >= 5)
            {
                UpdateWorldFrame();
                _snapshotUpdateClock.Restart();
            }

            //3.渲染
            if (_renderClock.ElapsedMilliseconds >= 40 && _currentFrame is { } frame)
            {
                _renderer.RenderFrame(frame);
                _renderClock.Restart();
            }

            Thread.Sleep(1);
        }

        //等待任务结束
        await _gameInputBroadcaster.WaitForCurrentPublishAsync();
    }


}
