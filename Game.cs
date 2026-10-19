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
    public GameState State{get; private set;}

    private int _score;
    private int _moveIntervalMs;
    private readonly Stopwatch _gamerunTimer = Stopwatch.StartNew();
    private readonly Stopwatch _foodCreateTimer = Stopwatch.StartNew();

    public Game()
    {
        this.State = GameState.Live;
        this._score = 0;
        _moveIntervalMs = 200;
    }


    private void HandleInput(Snake snake)
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            State = GameState.Exit;
            return;
        }

        if (key == ConsoleKey.Spacebar)
        {
            if (State == GameState.Live)
            {
                State = GameState.Pause;
                _gamerunTimer.Stop();
                _foodCreateTimer.Stop();
            }
            else if (State == GameState.Pause)
            {
                State = GameState.Live;
                _gamerunTimer.Start();
                _foodCreateTimer.Start();
            }
        }

        if (State != GameState.Pause)
        {
            Snake.Direction? newdiretion = key switch
            {
                ConsoleKey.UpArrow => Snake.Direction.Up,
                ConsoleKey.DownArrow => Snake.Direction.Down,
                ConsoleKey.LeftArrow => Snake.Direction.Left,
                ConsoleKey.RightArrow => Snake.Direction.Right,
                _ => null
            };
            if (newdiretion is { } direction)
            {
                snake.Turn(direction);
            }
        }

    }

    private void SpawnFoodIfDue(Snake snake)
    {
        bool isFoodSpawnDue = State == GameState.Live && _foodCreateTimer.ElapsedMilliseconds >= 5000;
        if (!isFoodSpawnDue)
        {
            return;
        }

        bool isOccupied;
        do
        {
            var newFood = new Point(
            Random.Shared.Next(2, Renderer.MapWidth - 2),
            Random.Shared.Next(2, Renderer.MapHeight - 2)
            );

            isOccupied = snake.Body.Contains(newFood) || _foodList.Contains(newFood);
            if (!isOccupied)
            {
                _foodList.Add(newFood);
            }
        } while (isOccupied);

        _foodCreateTimer.Restart();
    }

    private void CheckForCollision(Snake snake)
    {
        bool state_x = (snake.Head.X <= 0 || snake.Head.X >= Renderer.MapWidth - 1);
        bool state_y = (snake.Head.Y <= 0 || snake.Head.Y >= Renderer.MapHeight - 1);
        bool hitSelf = snake.Body.Skip(1).Contains(snake.Head);
        if (state_x || state_y || hitSelf)
        {
            State = GameState.Dead;
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
        var foodPosition = snake.Head;
        _foodList.Remove(foodPosition);
        _score += 3;
        if (_moveIntervalMs > 120)
        {
            _moveIntervalMs -= 5;
        }
    }

    private bool UpdateGameIfdue(Snake snake)
    {
        bool gamenext = State == GameState.Live && _gamerunTimer.ElapsedMilliseconds >= _moveIntervalMs;
        if (!gamenext)
        {
            return false;
        }
        SpawnFoodIfDue(snake);
        snake.Move();
        EatFood(snake);
        CheckForCollision(snake);

        _gamerunTimer.Restart();
        return true;
    }

    public void StartGame(Snake snake)
    {
        while (State is GameState.Live or GameState.Pause)
        {
            HandleInput(snake);
            if (State == GameState.Exit)
                break;

            if (UpdateGameIfdue(snake))
            {
                Renderer.RenderFrame(_score, _foodList, snake);
            }

            Thread.Sleep(10);
        }
    }

    public GameControl GameOver()
    {
        Renderer.DrawGameOver(_score);

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

