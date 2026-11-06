
using snake.Application;
using snake.Config;
using snake.Network;
using snake.Rendering.Skin;

namespace snake.Application.Startup;

internal static class AppRunner
{
    public static async Task RunAsync(string[] args)
    {
        // 1. 解析启动参数
        CommandLineOptions options = CommandLineOptions.Parse(args);

        // 2. 加载配置
        var (config, skinConfig) = AppConfigLoader.Load();

        // 3. Guest 模式
        if (options.Mode == AppRunMode.Guest)
        {
            MqttTransport guestNetwork =
                await ConnectAndSubscribeAsync(
                    options, "snake/world");

            WaitForStart("按 Enter 开始游戏...");

            GuestRunner guestRunner =
                new(config, skinConfig, guestNetwork);

            await guestRunner.RunGuestClient();
            return;
        }

        // 4. 单机 / Host 共用 GameRunner
        MqttTransport? network = null;

        if (options.Mode == AppRunMode.Host)
        {
            network = await ConnectAndSubscribeAsync(
                options, "snake/input");

            WaitForStart("按 Enter 启动游戏...");
        }

        GameRunner gameRunner = new(
            config,
            skinConfig,
            network
        );

        await RunInGameScreenAsync(gameRunner.RunGameAsync);
    }


    // 连接 MQTT 并订阅指定主题
    private static async Task<MqttTransport> ConnectAndSubscribeAsync(
        CommandLineOptions options,
        string topic)
    {
        MqttTransport network = new();

        await network.ConnectAsync(
            options.BrokerHost,
            options.BrokerPort
        );

        await network.SubscribeAsync(topic);

        Console.WriteLine(
            $"已连接 MQTT Broker:{options.BrokerHost}:{options.BrokerPort}");

        return network;
    }


    // 等待用户确认启动
    private static void WaitForStart(string message)
    {
        Console.WriteLine(message);
        Console.ReadLine();
    }


    // 游戏终端环境管理
    private static async Task RunInGameScreenAsync(Func<Task> runGame)
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
}
