using MQTTnet;
using System.Collections.Concurrent;
using System.Buffers;
public class MqttTransport
{
    private readonly IMqttClient _client;

    private readonly ConcurrentQueue<byte[]> _inbox = new();
 

    public MqttTransport()
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += e =>
        {
            byte[] data = e.ApplicationMessage.Payload.ToArray();
            _inbox.Enqueue(data);
            return Task.CompletedTask;
        };
    }

    //连接配置，调用 MQTTnet 建立连接
    public async Task ConnectAsync(string host, int port)
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .Build();

        await _client.ConnectAsync(options);//使用options参数连接
    }

    //创建订阅配置，然后让client订阅这个主题（options应该就是配置）
    public async Task SubscribeAsync(string topic)
    {
        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topic)
            .Build();
        await _client.SubscribeAsync(options);
    }

    //发布：装载信息成特定的类型，然后调用client的发布功能(输入的是byte的data)
    public async Task PublishAsync(string topic, byte[] data)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(data)
            .Build();

        await _client.PublishAsync(message);
    }

    //从inbox里面取出消息
    public bool TryReceive(out byte[]? data)
    {
        return _inbox.TryDequeue(out data);
    }

}