public static class Renderer
{
    public const int map_width = 40;
    public const int map_height = 20;
    private const int offset_x = 2;
    private const int offset_y = 3;


    private static void DrawCell(int x, int y, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(offset_x + x * 2, offset_y + y);
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
        int centerX = offset_x + (map_width - content.Length / 2);
        DrawText(centerX, y, color, content);
    }

    private static void DrawMap()
    {
        for (int y = 0; y < map_height; y++)
        {
            for (int x = 0; x < map_width; x++)
            {
                if (x == 0 || y == 0 || x == map_width - 1 || y == map_height - 1)
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


    private static void DrawFood(List<Point> Foodlist)
    {
        foreach (Point food in Foodlist)
        {
            DrawCell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }


    private static void DrawSnake(Snake snake)
    {
        DrawCell(snake.snake_head.X, snake.snake_head.Y, ConsoleColor.Blue);
        foreach (Point segment in snake.Body.Skip(1))
        {
            DrawCell(segment.X, segment.Y, ConsoleColor.Cyan);
        }
    }

    public static void DrawFrame(int score,List<Point> Foodlist, Snake snake)
    {
        Console.Clear();

        DrawMap();
        DrawHeader(score);
        DrawFooter();
        DrawFood(Foodlist);
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