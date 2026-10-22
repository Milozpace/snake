public class Renderer
{
    private readonly GameConfig config;
    private const int MapOriginX = 2;
    private const int MapOriginY = 3;

    public Renderer(GameConfig config)
    {
        this.config = config;
    }
    //工具方法
    private static void DrawGridCell(int x, int y, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(MapOriginX + x * 2, MapOriginY + y);
        Console.Write("██");
        Console.ResetColor();
    }

    private static void DrawText(int x, int y, ConsoleColor color, string content)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(x, y);
        Console.Write(content);
        Console.ResetColor();
    }

    private void DrawCenteredText(int y, ConsoleColor color, string content)
    {
        int centerX = MapOriginX + (config.MapWidth - content.Length / 2);
        DrawText(centerX, y, color, content);
    }

    private void DrawMap()
    {
        for (int y = 0; y < config.MapHeight; y++)
        {
            for (int x = 0; x < config.MapWidth; x++)
            {
                if (x == 0 || y == 0 || x == config.MapWidth - 1 || y == config.MapHeight - 1)
                {
                    DrawGridCell(x, y, ConsoleColor.DarkGray);
                }
            }
        }
    }

    //调用方法
    private void DrawHeader(int score)
    {
        DrawText(3, 1, ConsoleColor.DarkGreen, "S N A K E");
        DrawText(65, 1, ConsoleColor.White, $"S C O R E");
        DrawText(77, 1, ConsoleColor.DarkYellow, $"{score:D3}");
    }

    private void DrawFooter()
    {
        DrawText(3, 24, ConsoleColor.DarkCyan, "↑ ↓ ← →  MOVE");
        DrawText(74, 24, ConsoleColor.DarkGray, "ESC QUIT");
    }


    private void DrawFood(IReadOnlyList<Point> foodList)
    {
        foreach (Point food in foodList)
        {
            DrawGridCell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }


    private void DrawSnake(Snake snake)
    {
        DrawGridCell(snake.Head.X, snake.Head.Y, ConsoleColor.Blue);
        foreach (Point segment in snake.Body.Skip(1))
        {
            DrawGridCell(segment.X, segment.Y, ConsoleColor.Cyan);
        }
    }

    private void DrawPause()
    {
        DrawCenteredText(13, ConsoleColor.Blue, "P A U S E");
    }

    //提供的api
    public void RenderFrame(Game game, Snake snake)
    {
        Console.Clear();

        DrawMap();
        DrawHeader(game.Score);
        DrawFooter();
        DrawFood(game.FoodList);
        DrawSnake(snake);

        if (game.State == Game.GameState.Paused)
        {
            DrawPause();
        }
    }

    public void DrawGameOver(Game game)
    {
        DrawCenteredText(9, ConsoleColor.Red, "G A M E  O V E R");
        DrawCenteredText(12, ConsoleColor.White, $"SCORE: {game.Score}");
        DrawCenteredText(15, ConsoleColor.DarkGray, "'ENTER' TO RESTART");
        DrawCenteredText(16, ConsoleColor.DarkGray, " or 'ESC' TO EXIT");
    }



}