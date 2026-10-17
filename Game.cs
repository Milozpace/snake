using System.Data;
using System.Diagnostics;

class Game
{
    public enum GameState
    {
        live,
        dead,
        pause
    }

    public GameState gamestate;
    public int score;
    private Stopwatch Gamerun_timer = Stopwatch.StartNew();
    private Stopwatch Foodcreate_timer = Stopwatch.StartNew();

    public Game()
    {
        this.gamestate = GameState.live;
        this.score = 0;
    }
    public void StartGame(Snake snake, Renderer renderer)
    {
        while (gamestate == GameState.live)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                snake.currrentDirection = key switch
                {
                    ConsoleKey.UpArrow => Snake.Direction.Up,
                    ConsoleKey.DownArrow => Snake.Direction.Down,
                    ConsoleKey.LeftArrow => Snake.Direction.Left,
                    ConsoleKey.RightArrow => Snake.Direction.Right,
                    _ => snake.currrentDirection
                };

                if (key == ConsoleKey.Enter)
                {
                    break;
                }
            }

            //游戏运行/画面绘制循环(200ms)
            if (Gamerun_timer.ElapsedMilliseconds >= 200)
            {
                Console.Clear();
                renderer.draw_map();
                renderer.draw_food();
                renderer.draw_header(this.score);
                snake.move();
                renderer.draw_snake(snake);

                Gamerun_timer.Restart();
            }
            if (Foodcreate_timer.ElapsedMilliseconds >= 2000)
            {
                renderer.create_food(snake);
                Foodcreate_timer.Restart();
            }

            
            bool state_x = (snake.Snakehead.X <= 0 || snake.Snakehead.X >= Renderer.map_width - 1);
            bool state_y = (snake.Snakehead.Y <= 0 || snake.Snakehead.Y >= Renderer.map_height - 1);
            if (state_x || state_y)
            {
                gamestate = GameState.dead;
            }

            //游戏交互循环（10ms）
            Thread.Sleep(10);
        }
    }
}