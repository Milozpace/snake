
namespace snake.Rendering;

using System.Text;

public class ConsolePresenter
{
    private static readonly string[] AnsiForeground =
    [
        "\x1b[30m",
        "\x1b[34m",
        "\x1b[32m",
        "\x1b[36m",
        "\x1b[31m",
        "\x1b[35m",
        "\x1b[33m",
        "\x1b[37m",
        "\x1b[90m",
        "\x1b[94m",
        "\x1b[92m",
        "\x1b[96m",
        "\x1b[91m",
        "\x1b[95m",
        "\x1b[93m",
        "\x1b[97m"
    ];

    private static string ToAnsi(ConsoleColor color) =>
        AnsiForeground[(int)color];

    public void Present(
        FrameBuffer previousFrame,
        FrameBuffer currentFrame,
        bool fullRedraw)
    {
        StringBuilder output = new();

        if (fullRedraw)
            output.Append("\x1b[2J");

        for (int y = 0; y < currentFrame.Height; y++)
        {
            for (int x = 0; x < currentFrame.Width; x++)
            {
                Cell newCell = currentFrame.GetCell(x, y);
                Cell oldCell = previousFrame.GetCell(x, y);

                if (!fullRedraw && newCell == oldCell)
                    continue;

                output.Append($"\x1b[{y + 1};{x + 1}H");
                output.Append(ToAnsi(newCell.Color));
                output.Append(newCell.Character);
            }
        }

        output.Append("\x1b[0m");

        Console.Write(output.ToString());
    }
}
