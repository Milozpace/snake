using System.Data;
using System.Diagnostics;

class Game
{
    public enum GameState
    {
        live,
        dead,
        pause,
        exit
    }

    public enum GameControl
    {
        End,
        Restart
    }

    private readonly List<Point> Foodlist = [];
    private GameState gamestate;
    public GameState gameState => gamestate;
    private int score;
    private int GameRunspeed;
    private Stopwatch Gamerun_timer = Stopwatch.StartNew();
    private Stopwatch Foodcreate_timer = Stopwatch.StartNew();

    public Game()
    {
        this.gamestate = GameState.live;
        this.score = 0;
        GameRunspeed = 200;
    }


    private void HandleInput(Snake snake)
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            gamestate = GameState.exit;
            return;
        }

        if (key == ConsoleKey.Spacebar)
        {
            if (gamestate == GameState.live)
            {
                gamestate = GameState.pause;
                Gamerun_timer.Stop();
                Foodcreate_timer.Stop();
            }
            else if (gamestate == GameState.pause)
            {
                gamestate = GameState.live;
                Gamerun_timer.Start();
                Foodcreate_timer.Start();
            }
        }

        if (gamestate != GameState.pause)
        {
            Snake.Direction? Newdiretion = key switch
            {
                ConsoleKey.UpArrow => Snake.Direction.Up,
                ConsoleKey.DownArrow => Snake.Direction.Down,
                ConsoleKey.LeftArrow => Snake.Direction.Left,
                ConsoleKey.RightArrow => Snake.Direction.Right,
                _ => null
            };
            if (Newdiretion is { } direction)
            {
                snake.Turn(direction);
            }
        }

    }

    private void CreateFood(Snake snake)
    {
        bool Foodready = gamestate == GameState.live && Foodcreate_timer.ElapsedMilliseconds >= 5000;
        if (!Foodready)
        {
            return;
        }

        bool Incorrectfood;
        do
        {
            var new_food = new Point(
            Random.Shared.Next(2, Renderer.map_width - 2),
            Random.Shared.Next(2, Renderer.map_height - 2)
            );

            Incorrectfood = snake.Body.Contains(new_food) || Foodlist.Contains(new_food);
            if (!Incorrectfood)
            {
                Foodlist.Add(new_food);
            }
        } while (Incorrectfood);

        Foodcreate_timer.Restart();
    }

    private void DeadCheck(Snake snake)
    {
        bool state_x = (snake.snake_head.X <= 0 || snake.snake_head.X >= Renderer.map_width - 1);
        bool state_y = (snake.snake_head.Y <= 0 || snake.snake_head.Y >= Renderer.map_height - 1);
        bool hitSelf = snake.Body.Skip(1).Contains(snake.snake_head);
        if (state_x || state_y || hitSelf)
        {
            gamestate = GameState.dead;
        }
    }

    private void EatFood(Snake snake)
    {
        bool eatfood = Foodlist.Contains(snake.snake_head);
        if (!eatfood)
        {
            return;
        }

        snake.grow();
        var FoodPosition = snake.snake_head;
        Foodlist.Remove(FoodPosition);
        score += 3;
        if (GameRunspeed > 120)
        {
            GameRunspeed -= 5;
        }
    }

    private bool GameRun(Snake snake)
    {
        bool gamenext = gamestate == GameState.live && Gamerun_timer.ElapsedMilliseconds >= GameRunspeed;
        if (!gamenext)
        {
            return false;
        }
        CreateFood(snake);
        snake.move();
        EatFood(snake);
        DeadCheck(snake);

        Gamerun_timer.Restart();
        return true;
    }

    public void StartGame(Snake snake)
    {
        while (gamestate is GameState.live or GameState.pause)
        {
            HandleInput(snake);
            if (gamestate == GameState.exit)
                break;

            if (GameRun(snake))
            {
                Renderer.DrawFrame(score, Foodlist, snake);
            }

            Thread.Sleep(10);
        }
    }

    public GameControl GameOver()
    {
        Renderer.DrawGameover(score);

        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.Enter:
                    return GameControl.Restart;
                case ConsoleKey.Spacebar:
                    return GameControl.End;
                case ConsoleKey.Escape:
                    return GameControl.End;
            }
        }
    }
}

