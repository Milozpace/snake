namespace snake.Application.Startup;

internal enum AppRunMode
{
    SinglePlayer,
    Host,
    Guest
}

internal sealed record CommandLineOptions(
    AppRunMode Mode,
    string BrokerHost,
    int BrokerPort
)
{
    public static CommandLineOptions Parse(string[] args)
    {
        // 默认配置
        string brokerHost = "127.0.0.1";
        int brokerPort = 1883;

        // 解析 Broker
        int brokerIndex = Array.IndexOf(args, "--broker");

        if (brokerIndex >= 0)
        {
            if (brokerIndex + 1 >= args.Length ||
                args[brokerIndex + 1].StartsWith("--"))
            {
                throw new ArgumentException("缺少 Broker 地址");
            }

            brokerHost = args[brokerIndex + 1];

            // 可选端口
            if (brokerIndex + 2 < args.Length &&
                !args[brokerIndex + 2].StartsWith("--"))
            {
                if (!int.TryParse(args[brokerIndex + 2], out int port)
                    || port is < 1 or > 65535)
                {
                    throw new ArgumentException("无效的 Broker 端口");
                }

                brokerPort = port;
            }
        }

        // 解析启动模式
        AppRunMode mode =
            args.Contains("--proto-guest")
                ? AppRunMode.Guest
                : args.Contains("--proto-host")
                    ? AppRunMode.Host
                    : AppRunMode.SinglePlayer;

        return new CommandLineOptions(
            mode,
            brokerHost,
            brokerPort
        );
    }
}
