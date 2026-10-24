using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
    private readonly int FoodScore;

    public Snake snake1;
    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private long _lastUpdateMs = 0;
    private long _accumulatorMs = 0;

    public int SimStepMs { get; init; }

    private readonly int FoodSpawnIntervalMs;
    private int _foodSpawnCooldownMs;


    //config
    private readonly GameConfig config;


    //Constructor
    public Game(GameConfig config)
    {
        this.config = config;
        SimStepMs = config.SimStepMs;
        FoodSpawnIntervalMs = config.FoodSpawnIntervalMs;
        FoodScore = config.FoodScore;
        _foodSpawnCooldownMs = FoodSpawnIntervalMs;

        State = GameState.Playing;
        snake1 = new(config);
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
                    _clock.Stop();
                    break;
                }
            case GameState.Playing:
                {
                    _clock.Start();
                    break;
                }
        }
    }

    public void ToggleDirection(Snake snake, Snake.Direction direction)
    {
        snake.TryTurn(direction);
    }
    public void ToggleBoost()
    {
        snake1.TryBoost();
    }

    //处理游戏-接口
    public bool UpdateGameIfdue()
    {
        if (State != GameState.Playing)
            return false;

        long now = _clock.ElapsedMilliseconds;
        _accumulatorMs += now - _lastUpdateMs;
        _lastUpdateMs = now;

        if (_accumulatorMs > SimStepMs)
        {
            snake1.UpdateSnake();
            SpawnFoodIfDue();
            snake1.Move();

            CheckEatFood();
            CheckCollision();

            _accumulatorMs -= SimStepMs;
            return true;
        }
        else
            return false;


    }

    //游戏处理
    private void SpawnFoodIfDue()
    {
        _foodSpawnCooldownMs -= SimStepMs;
        if (_foodSpawnCooldownMs > 0)
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
        _foodSpawnCooldownMs += FoodSpawnIntervalMs;
    }

    private void CheckCollision()
    {
        bool state_x = (snake1.Head.X <= 0 || snake1.Head.X >= config.MapWidth - 1);
        bool state_y = (snake1.Head.Y <= 0 || snake1.Head.Y >= config.MapHeight - 1);
        bool hitSelf = snake1.Body.Skip(1).Contains(snake1.Head);
        if (state_x || state_y || hitSelf)
        {
            State = GameState.GameOver;
        }
    }

    private void CheckEatFood()
    {
        bool eatfood = _foodList.Contains(snake1.Head);
        if (!eatfood)
        {
            return;
        }

        snake1.Grow();
        var foodPosition = snake1.Head;
        _foodList.Remove(foodPosition);
        Score += FoodScore;
    }


}

