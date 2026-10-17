public class Renderer
{
    public const int map_width = 40;
    public const int map_height = 20;
    public const int offset_x = 2;
    public const int offset_y = 3;

    public List<Point> Foodlist = new();

    public bool correct_food = false;

    private void draw_cell(int x, int y, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.SetCursorPosition(offset_x + x * 2, offset_y + y);
        Console.Write("██");
    }

    public void draw_map()
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
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

        Console.ResetColor();
    }

    public void draw_header(int score)
    {
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.ForegroundColor = ConsoleColor.White;
        Console.SetCursorPosition(4, 1);
        Console.Write("S N A K E");

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.SetCursorPosition(20, 1);
        Console.Write($"SCORE    {score:D3}");

        Console.ResetColor();
    }

    public void create_food(Snake snake)
    {
        //感觉这里写的太麻烦了，对于食物生成的判断逻辑（后面再改！）
        while (!correct_food)
        {
            Random random = new Random();
            var new_food = new Point(
            random.Next(2, map_width - 2),
            random.Next(2, map_height - 2)
            );

            correct_food = true;
            foreach (Point segment in snake.body)
            {
                if (segment.X == new_food.X && segment.Y == new_food.Y)
                {
                    correct_food = false;
                }
            }
            if (correct_food)
            {
                Foodlist.Add(new_food);
            }
        }

        correct_food = false;
    }
    public void draw_food()
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
}