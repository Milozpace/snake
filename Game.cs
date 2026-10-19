using System.Data;
using System.Diagnostics;

class Game
{
    public enum GameState
    {
        Live,
        Dead,
        Pause,
        Exit
    }

    public enum GameControl
    {
        End,
        Restart
    }

    private readonly List<Point> _foodList = [];
    private GameState _gameState;
    public GameState State => _gameState;
    private int _score;
    private int _gameRunspeed;
    private Stopwatch _gamerunTimer = Stopwatch.StartNew();
    private Stopwatch _foodCreateTimer = Stopwatch.StartNew();

    public Game()
    {
        this._gameState = GameState.Live;
        this._score = 0;
        _gameRunspeed = 200;
    }


    private void HandleInput(Snake snake)
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            _gameState = GameState.Exit;
            return;
        }

        if (key == ConsoleKey.Spacebar)
        {
            if (_gameState == GameState.Live)
            {
                _gameState = GameState.Pause;
                _gamerunTimer.Stop();
                _foodCreateTimer.Stop();
            }
            else if (_gameState == GameState.Pause)
            {
                _gameState = GameState.Live;
                _gamerunTimer.Start();
                _foodCreateTimer.Start();
            }
        }

        if (_gameState != GameState.Pause)
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
        bool Foodready = _gameState == GameState.Live && _foodCreateTimer.ElapsedMilliseconds >= 5000;
        if (!Foodready)
        {
            return;
        }

        bool incorrectfood;
        do
        {
            var newFood = new Point(
            Random.Shared.Next(2, Renderer.MapWidth - 2),
            Random.Shared.Next(2, Renderer.MapHeight - 2)
            );

            incorrectfood = snake.Body.Contains(newFood) || _foodList.Contains(newFood);
            if (!incorrectfood)
            {
                _foodList.Add(newFood);
            }
        } while (incorrectfood);

        _foodCreateTimer.Restart();
    }

    private void DeadCheck(Snake snake)
    {
        bool state_x = (snake.Head.X <= 0 || snake.Head.X >= Renderer.MapWidth - 1);
        bool state_y = (snake.Head.Y <= 0 || snake.Head.Y >= Renderer.MapHeight - 1);
        bool hitSelf = snake.Body.Skip(1).Contains(snake.Head);
        if (state_x || state_y || hitSelf)
        {
            _gameState = GameState.Dead;
        }
    }

    private void EatFood(Snake snake)
    {
        bool eatfood = _foodList.Contains(snake.Head);
        if (!eatfood)
        {
            return;
        }

        snake.Grow();
        var FoodPosition = snake.Head;
        _foodList.Remove(FoodPosition);
        _score += 3;
        if (_gameRunspeed > 120)
        {
            _gameRunspeed -= 5;
        }
    }

    private bool GameRun(Snake snake)
    {
        bool gamenext = _gameState == GameState.Live && _gamerunTimer.ElapsedMilliseconds >= _gameRunspeed;
        if (!gamenext)
        {
            return false;
        }
        CreateFood(snake);
        snake.Move();
        EatFood(snake);
        DeadCheck(snake);

        _gamerunTimer.Restart();
        return true;
    }

    public void StartGame(Snake snake)
    {
        while (_gameState is GameState.Live or GameState.Pause)
        {
            HandleInput(snake);
            if (_gameState == GameState.Exit)
                break;

            if (GameRun(snake))
            {
                Renderer.DrawFrame(_score, _foodList, snake);
            }

            Thread.Sleep(10);
        }
    }

    public GameControl GameOver()
    {
        Renderer.DrawGameover(_score);

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

