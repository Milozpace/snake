using System.Data;
using System.Diagnostics;

public class Game
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }
    public GameState State { get; private set; }
    public int Score { get; private set; }

    public Snake snake1;
    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    private readonly Stopwatch _gamerunTimer = Stopwatch.StartNew();
    private readonly Stopwatch _foodCreateTimer = Stopwatch.StartNew();

    //config
    private readonly GameConfig config;
    private int GameMoveIntervalMs;

    //Constructor
    public Game(GameConfig config)
    {
        this.config = config;
        Score = 0;
        State = GameState.Playing;

        snake1 = new();
        GameMoveIntervalMs = config.InitMoveIntervalMs;
    }

    //键盘输入接口
    public void TogglePause()
    {
        State = State switch
        {
            GameState.Playing => GameState.Paused,
            GameState.Paused => GameState.Playing,
            _ => State
        };
        switch (State)
        {
            case GameState.Paused:
                {
                    _gamerunTimer.Stop();
                    _foodCreateTimer.Stop();
                    break;
                }
            case GameState.Playing:
                {
                    _gamerunTimer.Start();
                    _foodCreateTimer.Start();
                    break;
                }
        }
        ;
    }

    public void ToggleDirection(Snake snake, Snake.Direction direction)
    {
        snake.TryTurn(direction);
    }

    //处理游戏-接口
    public bool UpdateGameIfdue()
    {
        bool gamenext = State == GameState.Playing && _gamerunTimer.ElapsedMilliseconds >= GameMoveIntervalMs;
        if (!gamenext)
        {
            return false;
        }

        SpawnFoodIfDue();
        snake1.Move();
        EatFood();
        CheckForCollision();

        _gamerunTimer.Restart();
        return true;
    }

    //游戏处理
    private void SpawnFoodIfDue()
    {
        bool isFoodSpawnDue = State == GameState.Playing && _foodCreateTimer.ElapsedMilliseconds >= config.FoodSpawnIntervalMs;
        if (!isFoodSpawnDue)
        {
            return;
        }

        bool isOccupied;
        do
        {
            var newFood = new Point(
            Random.Shared.Next(2, config.MapWidth - 2),
            Random.Shared.Next(2, config.MapHeight - 2)
            );

            isOccupied = snake1.Body.Contains(newFood) || _foodList.Contains(newFood);
            if (!isOccupied)
            {
                _foodList.Add(newFood);
            }
        } while (isOccupied);

        _foodCreateTimer.Restart();
    }

    private void CheckForCollision()
    {
        bool state_x = (snake1.Head.X <= 0 || snake1.Head.X >= config.MapWidth - 1);
        bool state_y = (snake1.Head.Y <= 0 || snake1.Head.Y >= config.MapHeight - 1);
        bool hitSelf = snake1.Body.Skip(1).Contains(snake1.Head);
        if (state_x || state_y || hitSelf)
        {
            State = GameState.GameOver;
        }
    }

    private void EatFood()
    {
        bool eatfood = _foodList.Contains(snake1.Head);
        if (!eatfood)
        {
            return;
        }

        snake1.Grow();
        var foodPosition = snake1.Head;
        _foodList.Remove(foodPosition);
        Score += 3;
        if (GameMoveIntervalMs > 120)
        {
            GameMoveIntervalMs -= 5;
        }
    }


}

