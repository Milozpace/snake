namespace snake.Network;

using SnakeNetwork.Protocol;
using Google.Protobuf;
using System.Diagnostics;

public class GameInputBroadcaster(MqttTransport network)
{
    //依赖
    private readonly MqttTransport _network = network;

    //时间
    private readonly Stopwatch _inputBroadcastClock = Stopwatch.StartNew();

    //发布
    private readonly Queue<PlayerInput> _pendingInputs = new();
    private Task? _inputPublishTask;



    public void EnqueueInput(PlayerInput input)
        => _pendingInputs.Enqueue(input);

    private Task PublicshInputAsync()
    {
        PlayerInput nextInput = _pendingInputs.Dequeue();
        byte[] inputPublish = nextInput.ToByteArray();
        _inputPublishTask = _network.PublishAsync("snake/input", inputPublish);
        return _inputPublishTask;
    }

    public void UpdateInputPublishing()
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

        if (_inputBroadcastClock.ElapsedMilliseconds < 5)
            return;

        _inputPublishTask = PublicshInputAsync();
        _inputBroadcastClock.Restart();
    }
    public Task WaitForCurrentPublishAsync()
    => _inputPublishTask ?? Task.CompletedTask;
}