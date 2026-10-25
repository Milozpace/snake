using System.Reflection.PortableExecutable;
using System.Text;

public class Renderer
{
    private readonly GameConfig config;
    private const int MapOriginX = 2;
    private const int MapOriginY = 3;

    private static readonly string[] AnsiForeground =
    [
        "\x1b[30m", // Black
        "\x1b[34m", // DarkBlue
        "\x1b[32m", // DarkGreen
        "\x1b[36m", // DarkCyan
        "\x1b[31m", // DarkRed
        "\x1b[35m", // DarkMagenta
        "\x1b[33m", // DarkYellow
        "\x1b[37m", // Gray
        "\x1b[90m", // DarkGray
        "\x1b[94m", // Blue
        "\x1b[92m", // Green
        "\x1b[96m", // Cyan
        "\x1b[91m", // Red
        "\x1b[95m", // Magenta
        "\x1b[93m", // Yellow
        "\x1b[97m", // White
    ];
    private static string ToAnsi(ConsoleColor color) =>
        AnsiForeground[(int)color];

    private FrameBuffer currentFrame;
    private FrameBuffer previousFrame;
    private bool isFirstFrame;

    public Renderer(GameConfig config)
    {
        this.config = config;
        currentFrame = new(
            MapOriginX + config.MapWidth * 2 + 10,
            MapOriginY + config.MapHeight + 5
        );
        previousFrame = new(
            MapOriginX + config.MapWidth * 2 + 10,
            MapOriginY + config.MapHeight + 5
        );

        isFirstFrame = true;
    }
    //工具方法
    private void DrawCanvaCell(int x, int y, ConsoleColor color)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        currentFrame.SetCell(screenX, screenY, new('█', color));
        currentFrame.SetCell(screenX + 1, screenY, new('█', color));
    }

    private void DrawCanvaText(int x, int y, ConsoleColor color, string content)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        for (int i = 0; i < content.Length; i++)
        {
            char text = content[i];
            currentFrame.SetCell(screenX + i, screenY, new(text, color));
        }
    }

    private void DrawScreenText(int screenX, int screenY, ConsoleColor color, string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            currentFrame.SetCell(screenX + i, screenY, new(content[i], color));
        }
    }

    private void DrawMapRowText(int mapCharX, int mapRow, ConsoleColor color, string content)
    {
        DrawScreenText(MapOriginX + mapCharX, MapOriginY + mapRow, color, content);
    }

    private void DrawSkillMeter(
        double progress,
        string label,
        ConsoleColor fillColor,
        ConsoleColor textColor)
    {
        const int labelWidth = 9;

        int mapWidthChars = config.MapWidth * 2;
        int reservedWidth = 7 + 2 + labelWidth;
        int maxBarWidth = Math.Max(1, mapWidthChars - reservedWidth);
        int barWidth = Math.Clamp(config.MapWidth / 2, 1, Math.Min(16, maxBarWidth));

        progress = Math.Clamp(progress, 0.0, 1.0);

        int filled = (int)Math.Floor(progress * barWidth);
        if (progress > 0 && filled == 0)
            filled = 1;
        filled = Math.Clamp(filled, 0, barWidth);

        int blockWidth = 7 + barWidth + 2 + labelWidth;
        int x = Math.Max(0, mapWidthChars - blockWidth);
        int row = config.MapHeight + 1;

        DrawMapRowText(x, row, ConsoleColor.DarkGray, "BOOST [");
        x += 7;

        DrawMapRowText(x, row, fillColor, new string('█', filled));
        x += filled;

        DrawMapRowText(x, row, ConsoleColor.DarkGray, new string('░', barWidth - filled));
        x += barWidth - filled;

        DrawMapRowText(x, row, ConsoleColor.DarkGray, "] ");
        x += 2;

        DrawMapRowText(x, row, textColor, label);

        int padding = labelWidth - label.Length;
        if (padding > 0)
        {
            DrawMapRowText(
                x + label.Length,
                row,
                ConsoleColor.DarkGray,
                new string(' ', padding));
        }
    }

    private void DrawCenteredText(int y, ConsoleColor color, string content)
    {
        int screenCenterX = (config.MapWidth * 2 - content.Length) / 4; ;

        DrawCanvaText(screenCenterX, y, color, content);
    }

    private static string FormatCooldown(double progress)
    {
        progress = Math.Clamp(progress, 0.0, 1.0);

        if (progress < 0.20)
            return "0%";

        if (progress < 0.40)
            return "20%";

        if (progress < 0.60)
            return "40%";

        if (progress < 0.80)
            return "60%";

        double percent = progress * 100.0;

        if (percent > 99.9)
            percent = 99.9;

        return $"{percent:0.0}%";
    }

    //调用方法
    private void DrawMap()
    {
        for (int y = 0; y < config.MapHeight; y++)
        {
            for (int x = 0; x < config.MapWidth; x++)
            {
                if (x == 0 || y == 0 || x == config.MapWidth - 1 || y == config.MapHeight - 1)
                {
                    DrawCanvaCell(x, y, ConsoleColor.DarkGray);
                }
            }
        }
    }
    private void DrawHeader(int score)
    {
        DrawCanvaText(1, -1, ConsoleColor.DarkGreen, "S N A K E");
        DrawCanvaText(config.MapWidth - 10, -1, ConsoleColor.White, $"S C O R E");
        DrawCanvaText(config.MapWidth - 4, -1, ConsoleColor.DarkYellow, $"{score:D3}");
    }

    private void DrawFooter()
    {
        DrawCanvaText(1, config.MapHeight, ConsoleColor.DarkCyan, "↑ ↓ ← →  MOVE");
        DrawCanvaText(1, config.MapHeight + 2, ConsoleColor.DarkGray, "ESC QUIT");
    }

    private void DrawSkillbar(Snake snake)
    {
        if (snake.CurrentSkillState == Snake.SkillState.Ready)
        {
            DrawSkillMeter(
                progress: 1.0,
                label: "READY [M]",
                fillColor: ConsoleColor.Green,
                textColor: ConsoleColor.Green);
            return;
        }

        if (snake.CurrentSkillState == Snake.SkillState.Active)
        {
            double progress =
                (double)snake._skillActiveTimerMs / snake.SkillActiveMs;

            double seconds = snake._skillActiveTimerMs / 1000.0;

            DrawSkillMeter(
                progress: progress,
                label: $"{seconds:0.0}s",
                fillColor: ConsoleColor.Yellow,
                textColor: ConsoleColor.Yellow);
            return;
        }

        if (snake.CurrentSkillState == Snake.SkillState.CoolingDown)
        {
            double progress =
                1.0 - (double)snake._skillCooldownTimerMs / snake.SkillCooldownMs;

            string label = FormatCooldown(progress);

            ConsoleColor color = progress >= 0.80
                ? ConsoleColor.Green
                : ConsoleColor.Gray;

            DrawSkillMeter(
                progress: progress,
                label: label,
                fillColor: color,
                textColor: color);
        }
    }

    private void DrawFood(IReadOnlyList<Point> foodList)
    {
        foreach (Point food in foodList)
        {
            DrawCanvaCell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }


    private void DrawSnake(Snake snake)
    {
        DrawCanvaCell(snake.Head.X, snake.Head.Y, ConsoleColor.Blue);
        foreach (Point segment in snake.Body.Skip(1))
        {
            DrawCanvaCell(segment.X, segment.Y, ConsoleColor.Cyan);
        }
    }

    private void DrawPause()
    {
        DrawCenteredText(config.MapHeight / 2, ConsoleColor.Blue, "P A U S E");
    }

    private void DrawGameOver(Game game)
    {
        int centerY = config.MapHeight / 2;
        DrawCenteredText(centerY - 3, ConsoleColor.Red, "G A M E  O V E R");
        DrawCenteredText(centerY, ConsoleColor.White, $"SCORE: {game.Score}");
        DrawCenteredText(centerY + 3, ConsoleColor.DarkGray, "'ENTER' TO RESTART");
        DrawCenteredText(centerY + 4, ConsoleColor.DarkGray, " or 'ESC' TO EXIT");
    }
    private void DrawCanvas(Game game, Snake snake)
    {
        DrawMap();
        DrawHeader(game.Score);
        DrawFooter();
        DrawFood(game.FoodList);
        DrawSnake(snake);
        DrawSkillbar(snake);
    }


    private void Present(FrameBuffer previousFrame, FrameBuffer currentFrame)
    {
        StringBuilder output = new();

        bool fullRedraw = isFirstFrame;

        if (fullRedraw)
        {
            output.Append("\x1b[2J");
        }

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

        isFirstFrame = false;
    }

    //API
    public void RenderFrame(Game game, Snake snake)
    {
        currentFrame.Clear();
        DrawCanvas(game, snake);

        if (game.State == Game.GameState.Paused)
            DrawPause();
        if (game.State == Game.GameState.GameOver)
            DrawGameOver(game);

        Present(previousFrame, currentFrame);
        (previousFrame, currentFrame) = (currentFrame, previousFrame);
    }

}