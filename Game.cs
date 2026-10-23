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

    public Snake snake1;
    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private long _lastUpdateMs = 0;
    private long _accumulatorMs = 0;

    public int SimStepMs { get; init; }

    private readonly int InitFoodSpawnIntervalMs = 5000;
    private int _foodSpawnCooldownMs;


    //config
    private readonly GameConfig config;
    private int GameMoveIntervalMs;

    //Constructor
    public Game(GameConfig config)
    {
        this.config = config;
        Score = 0;
        State = GameState.Playing;

        SimStepMs = 5;

        snake1 = new(config);
        GameMoveIntervalMs = config.InitMoveIntervalMs;
        _foodSpawnCooldownMs = InitFoodSpawnIntervalMs;
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

    //处理游戏-接口
    //////////////////////////////////////！！！////////////////////（5ms步长调用点）
    public bool UpdateGameIfdue()
    {
        if (State != GameState.Playing)
        {
            return false;
        }
        long now = _clock.ElapsedMilliseconds;
        _accumulatorMs += now - _lastUpdateMs;
        _lastUpdateMs = now;

        if (_accumulatorMs > SimStepMs)
        {
            //由游戏按更新步长更新游戏状态啦🐱
            SpawnFoodIfDue();
            snake1.Move();
            //但是后面应该把更新接口独立更新一下，放在最前面，而不是让move和spawn方法去管理

            //以下也是dirty才调用（但并dirty重置需要在renderer处清零）
            EatFood();
            CheckForCollision();

            _accumulatorMs += SimStepMs;
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
        _foodSpawnCooldownMs += InitFoodSpawnIntervalMs;
        //dirty? 感觉现在就很需要一些记录这些变化的常量吧
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

