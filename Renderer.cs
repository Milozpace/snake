public static class Renderer
{
    public const int MapWidth = 40;
    public const int MapHeight = 20;
    private const int OffsetX = 2;
    private const int OffsetY = 3;


    private static void DrawCell(int x, int y, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(OffsetX + x * 2, OffsetY + y);
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

    private static void DrawCenterText(int y, ConsoleColor color, string content)
    {
        int centerX = OffsetX + (MapWidth - content.Length / 2);
        DrawText(centerX, y, color, content);
    }

    private static void DrawMap()
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                if (x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1)
                {
                    DrawCell(x, y, ConsoleColor.DarkGray);
                }
            }
        }
    }

    private static void DrawHeader(int score)
    {
        DrawText(3, 1, ConsoleColor.DarkGreen, "S N A K E");
        DrawText(65, 1, ConsoleColor.White, $"S C O R E");
        DrawText(77, 1, ConsoleColor.DarkYellow, $"{score:D3}");
    }

    private static void DrawFooter()
    {
        DrawText(3, 24, ConsoleColor.DarkCyan, "↑ ↓ ← →  MOVE");
        DrawText(74, 24, ConsoleColor.DarkGray, "ESC QUIT");
    }


    private static void DrawFood(List<Point> foodList)
    {
        foreach (Point food in foodList)
        {
            DrawCell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }


    private static void DrawSnake(Snake snake)
    {
        DrawCell(snake.Head.X, snake.Head.Y, ConsoleColor.Blue);
        foreach (Point segment in snake.Body.Skip(1))
        {
            DrawCell(segment.X, segment.Y, ConsoleColor.Cyan);
        }
    }

    public static void DrawFrame(int score,List<Point> foodList, Snake snake)
    {
        Console.Clear();

        DrawMap();
        DrawHeader(score);
        DrawFooter();
        DrawFood(foodList);
        DrawSnake(snake);
    }
    public static void DrawGameover(int score)
    {
        DrawCenterText(9, ConsoleColor.Red, "G A M E  O V E R");
        DrawCenterText(12, ConsoleColor.White, $"SCORE: {score}");
        DrawCenterText(15, ConsoleColor.DarkGray, "'ENTER' TO RESTART");
        DrawCenterText(16, ConsoleColor.DarkGray, " or 'SPACE' TO EXIT");
    }

}