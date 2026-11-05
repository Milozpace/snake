using System.Text.Json;
using System.Text.Json.Serialization;
using snake.Network;
using snake.Application;
using snake.Rendering.Skin;
using snake.Config;

string baseDir = AppContext.BaseDirectory;

string gamePath = Path.Combine(baseDir, "Config", "configClassic.json");
string skinPath = Path.Combine(baseDir, "Rendering", "Skin", "skins.json");

string GameJson = File.ReadAllText(gamePath);
string SkinsJson = File.ReadAllText(skinPath);

GameConfig config =
    JsonSerializer.Deserialize<GameConfig>(GameJson)
    ?? throw new InvalidDataException("Invalid game configuration");

JsonSerializerOptions skinJsonOptions = new();
skinJsonOptions.Converters.Add(new JsonStringEnumConverter());

SkinConfig skinConfig = JsonSerializer.Deserialize<SkinConfig>(
        SkinsJson,
        skinJsonOptions
    )
    ?? throw new InvalidDataException("Invalid skin configuration");

///////////////////////////////////////////////////////

string brokerHost = "127.0.0.1";
int brokerPort = 1883;
int brokerIndex = Array.IndexOf(args, "--broker");

if (brokerIndex >= 0)
{
    if (brokerIndex + 1 >= args.Length ||
    args[brokerIndex + 1].StartsWith("--"))
    {
        throw new ArgumentException("缺少 Broker 地址");
    }

    brokerHost = args[brokerIndex + 1];


    if (brokerIndex + 2 < args.Length &&
        !args[brokerIndex + 2].StartsWith("--"))//先判断有没有提供“可能正确的port”
    {
        if (!int.TryParse(args[brokerIndex + 2], out int port)
            || port is < 1 or > 65535)
        {
            throw new ArgumentException("无效的 Broker 端口");
        }

        brokerPort = port;
    }
}

//GUEST
if (args.Contains("--proto-guest"))
{
    var guest = new MqttTransport();

    await guest.ConnectAsync(brokerHost, brokerPort);
    await guest.SubscribeAsync("snake/world");

    Console.WriteLine($"已连接 MQTT Broker:{brokerHost}:{brokerPort}");
    Console.WriteLine("按 Enter 开始游戏...");
    Console.ReadLine();

    GuestRunner guestRunner = new(config, skinConfig, guest);
    await guestRunner.RunGuestClient();
}

//HOST
else if (args.Contains("--proto-host"))
{
    var host = new MqttTransport();

    await host.ConnectAsync(brokerHost, brokerPort);
    await host.SubscribeAsync("snake/input");

    Console.WriteLine($"已连接 MQTT Broker:{brokerHost}:{brokerPort}");
    Console.WriteLine("按 Enter 启动游戏...");
    Console.ReadLine();

    GameRunner gameRunner = new(config, skinConfig, host);
    await RunInGameScreenAsync(gameRunner.RungameAsync);
}
//正常游戏
else
{
    GameRunner gameRunner = new(config, skinConfig);
    await RunInGameScreenAsync(gameRunner.RungameAsync);
}


static async Task RunInGameScreenAsync(Func<Task> runGame)
{
    try
    {
        Console.Write("\x1b[?1049h");
        Console.Clear();
        Console.CursorVisible = false;

        await runGame();
    }
    finally
    {
        Console.CursorVisible = true;
        Console.Clear();
        Console.Write("\x1b[?1049l");
    }
}