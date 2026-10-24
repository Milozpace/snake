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

    private void DrawCenteredText(int y, ConsoleColor color, string content)
    {
        int screenCenterX = (config.MapWidth * 2 - content.Length) / 4; ;

        DrawCanvaText(screenCenterX, y, color, content);
    }

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

    //调用方法
    private void DrawHeader(int score)
    {
        DrawCanvaText(1, -1, ConsoleColor.DarkGreen, "S N A K E");
        DrawCanvaText(config.MapWidth - 10, -1, ConsoleColor.White, $"S C O R E");
        DrawCanvaText(config.MapWidth - 4, -1, ConsoleColor.DarkYellow, $"{score:D3}");
    }

    private void DrawFooter()
    {
        DrawCanvaText(1, 20, ConsoleColor.DarkCyan, "↑ ↓ ← →  MOVE");
        DrawCanvaText(config.MapWidth - 5, 20, ConsoleColor.DarkGray, "ESC QUIT");
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