using System.Diagnostics;
using snake.Core;
using Google.Protobuf;
using snake.Config;

namespace snake.Network;


public class WorldSnapshotBroadcaster(GameConfig config, MqttTransport network)
{
    //依赖
    private readonly GameConfig _config = config;
    private readonly MqttTransport _network = network;

    //任务管理
    private Task? _snapshotPublishTask;
    private ulong _sequence;
    private string? _publishError;
    //时间管理
    private readonly Stopwatch _snapshotClock = Stopwatch.StartNew();
    private const int NormalIntervalMs = 50;
    private const int RetryIntervalMs = 500;


    private async Task PublishSnapshotAsync(Game game)
    {
        _sequence++;
        var worldSnapshot = WorldSnapshotFactory.ToWorldSnapshot(_config, game, _sequence);
        byte[] worldData = worldSnapshot.ToByteArray();
        await _network.PublishAsync("snake/world", worldData);
    }

    public void UpdateSnapshotBroadcast(Game game)//核心处理控制发布任务的调用
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

        int intervalMs = _publishError is null
            ? NormalIntervalMs
            : RetryIntervalMs;


        if (_snapshotClock.ElapsedMilliseconds < intervalMs)
            return;

        if (_snapshotPublishTask is { IsCompleted: false })
            return;

        _snapshotPublishTask = PublishSnapshotAsync(game);
        _snapshotClock.Restart();
    }

    public async Task PublishFinalSnapshotAsync(Game game)
    {
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

        await PublishSnapshotAsync(game);
    }
}

