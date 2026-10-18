public class Renderer
{
    public const int map_width = 40;
    public const int map_height = 20;
    private const int offset_x = 2;
    private const int offset_y = 3;


    private void draw_cell(int x, int y, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(offset_x + x * 2, offset_y + y);
        Console.Write("██");
        Console.ResetColor();
    }

    private void draw_text(int x, int y, ConsoleColor color, string content)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(x, y);
        Console.Write(content);
        Console.ResetColor();
    }

    private void draw_text_centerX(int y, ConsoleColor color, string content)
    {
        //让文字水平居中
        int centerX = offset_x + (map_width - content.Length / 2);
        draw_text(centerX, y, color, content);
    }

    public void draw_map()
    {
        for (int y = 0; y < map_height; y++)
        {
            for (int x = 0; x < map_width; x++)
            {
                if (x == 0 || y == 0 || x == map_width - 1 || y == map_height - 1)
                {
                    draw_cell(x, y, ConsoleColor.Green);
                }
            }
        }
    }

    public void draw_header(int score)
    {
        draw_text(4, 1, ConsoleColor.White, "S N A K E");
        draw_text(20, 1, ConsoleColor.White, $"SCORE    {score:D3}");
    }


    public void draw_food(List<Point> Foodlist)
    {
        foreach (Point food in Foodlist)
        {
            draw_cell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }


    public void draw_snake(Snake snake)
    {
        foreach (Point segment in snake.body)
        {
            draw_cell(segment.X, segment.Y, ConsoleColor.Cyan);
        }
    }

    public void draw_gameover(int score)
    {
        draw_text_centerX(9, ConsoleColor.Red, "G A M E  O V E R");
        draw_text_centerX(12, ConsoleColor.White, $"SCORE: {score}");
        draw_text_centerX(15, ConsoleColor.DarkGray, "ENTER TO RESTART");
    }
}