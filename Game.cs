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

    public List<Point> Foodlist = [];

    public GameState gamestate;
    public int score;
    public int GameRunspeed;
    private Stopwatch Gamerun_timer = Stopwatch.StartNew();
    private Stopwatch Foodcreate_timer = Stopwatch.StartNew();

    public Game()
    {
        this.gamestate = GameState.live;
        this.score = 0;
        GameRunspeed = 200;
    }
    public void Create_food(Snake snake)
    {
        bool correct_food = false;
        do
        {
            Random random = new Random();
            var new_food = new Point(
            random.Next(2, Renderer.map_width - 2),
            random.Next(2, Renderer.map_height - 2)
            );

            correct_food = true;
            if (snake.body.Contains(new_food))
            {
                correct_food = false;
            }
            if (correct_food)
            {
                Foodlist.Add(new_food);
            }
        } while (!correct_food);
    }
    //游戏主循环
    public void StartGame(Snake snake, Renderer renderer)
    {
        while (gamestate == GameState.live || gamestate == GameState.pause)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                //暂停与退出判断
                if (key == ConsoleKey.Escape)
                {
                    gamestate = GameState.exit;
                    break;
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

                //方向
                if (gamestate != GameState.pause)
                {
                    Snake.Direction Newdiretion = key switch
                    {
                        ConsoleKey.UpArrow => Snake.Direction.Up,
                        ConsoleKey.DownArrow => Snake.Direction.Down,
                        ConsoleKey.LeftArrow => Snake.Direction.Left,
                        ConsoleKey.RightArrow => Snake.Direction.Right,
                        _ => snake.currrentDirection
                    };

                    switch (Newdiretion)
                    {
                        case Snake.Direction.Up when (snake.currrentDirection != Snake.Direction.Down):
                            snake.currrentDirection = Newdiretion;
                            break;

                        case Snake.Direction.Down when (snake.currrentDirection != Snake.Direction.Up):
                            snake.currrentDirection = Newdiretion;
                            break;

                        case Snake.Direction.Left when (snake.currrentDirection != Snake.Direction.Right):
                            snake.currrentDirection = Newdiretion;
                            break;
                        case Snake.Direction.Right when (snake.currrentDirection != Snake.Direction.Left):
                            snake.currrentDirection = Newdiretion;
                            break;
                    }
                }

            }

            //游戏运行/画面绘制循环(200ms)
            if (gamestate == GameState.live && Gamerun_timer.ElapsedMilliseconds >= GameRunspeed)
            {
                Console.Clear();
                renderer.draw_map();
                renderer.draw_header(this.score);
                renderer.draw_footer();
                snake.move(Foodlist);

                if (Foodlist.Contains(snake.Snakehead))
                {
                    snake.grow();
                    var FoodPosition = snake.Snakehead;
                    Foodlist.Remove(FoodPosition);
                    score += 3;
                    if (GameRunspeed > 120)
                        GameRunspeed -= 5;
                }

                renderer.draw_food(Foodlist);
                renderer.draw_snake(snake);
                Gamerun_timer.Restart();
            }

            //食物生成循环（5s）
            if (gamestate == GameState.live && Foodcreate_timer.ElapsedMilliseconds >= 5000)
            {
                Create_food(snake);
                Foodcreate_timer.Restart();
            }

            //死亡检查点
            bool state_x = (snake.Snakehead.X <= 0 || snake.Snakehead.X >= Renderer.map_width - 1);
            bool state_y = (snake.Snakehead.Y <= 0 || snake.Snakehead.Y >= Renderer.map_height - 1);
            bool hitSelf = snake.body.Skip(1).Contains(snake.Snakehead);
            if (state_x || state_y || hitSelf)
            {
                gamestate = GameState.dead;
            }

            //游戏交互循环（10ms）
            Thread.Sleep(10);
        }
    }

    public GameControl GameOver(Renderer renderer)
    {
        renderer.draw_gameover(score);

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

