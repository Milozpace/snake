using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using SnakeNetwork.Protocol;


if (args.Contains("--proto-guest"))
{
    var guest = new MqttTransport();

    string brokerHost = "127.0.0.1";
    int brokerPort = 1883;

    await guest.ConnectAsync(brokerHost, brokerPort);
    await guest.SubscribeAsync("snake/world");

    Console.WriteLine($"已连接 MQTT Broker:{brokerHost}:{brokerPort}");
    Console.WriteLine("按 Enter 开始游戏...");
    Console.ReadLine();

    while (true)
    {
        while (guest.TryReceive(out byte[]? received) && received is not null)
        {
            WorldSnapshot snapshot = WorldSnapshot.Parser.ParseFrom(received);

            Console.WriteLine(
                $"Seq: {snapshot.Sequence}, " +
                $"Players: {snapshot.Players.Count}, " +
                $"Foods: {snapshot.Foods.Count}"
            );
        }

        if (!Console.KeyAvailable)
        {
            await Task.Delay(20);
            continue;
        }

        ConsoleKey key = Console.ReadKey(true).Key;
        if (key is ConsoleKey.Enter)
            break;


        bool boost = false;
        if (key is ConsoleKey.Q or ConsoleKey.M)
            boost = true;

        Direction inputDirection = key switch
        {
            ConsoleKey.UpArrow => Direction.Up,
            ConsoleKey.DownArrow => Direction.Down,
            ConsoleKey.LeftArrow => Direction.Left,
            ConsoleKey.RightArrow => Direction.Right,
            _ => Direction.Unspecified
        };

        var input = new PlayerInput()
        {
            PlayId = 2,
            Direction = inputDirection,
            Boost = boost
        };

        byte[] data = input.ToByteArray();
        await guest.PublishAsync("snake/input", data);
    }
    Console.ReadLine();
    return;
}



if (args.Contains("--proto-host"))
{
    var host = new MqttTransport();

    string brokerHost = "127.0.0.1";
    int brokerPort = 1883;
    await host.ConnectAsync(brokerHost, brokerPort);
    await host.SubscribeAsync("snake/input");

    Console.WriteLine($"已连接 MQTT Broker:{brokerHost}:{brokerPort}");
    Console.WriteLine("按 Enter 启动游戏...");
    Console.ReadLine();

    string GameJson = File.ReadAllText("./Config/configClassic.json");
    string SkinsJson = File.ReadAllText("./Skin/skins.json");
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


    Console.Write("\x1b[?1049h");
    Console.Clear();
    Console.CursorVisible = false;

    GameRunner gameRunner = new(config, skinConfig, host);
    gameRunner.Rungame();

    Console.CursorVisible = true;
    Console.Clear();
    Console.Write("\x1b[?1049l");

    return;
}
/////////////////////////////////////////////////////////////////////
else
{
    string GameJson = File.ReadAllText("./Config/configClassic.json");
    string SkinsJson = File.ReadAllText("./Skin/skins.json");
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


    Console.Write("\x1b[?1049h");
    Console.Clear();
    Console.CursorVisible = false;

    GameRunner gameRunner = new(config, skinConfig);
    gameRunner.Rungame();

    Console.CursorVisible = true;
    Console.Clear();
    Console.Write("\x1b[?1049l");
}
