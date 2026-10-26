using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public class Game
{
    //游戏状态
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }
    public GameState State { get; private set; }

    //游戏成员

    private Snake snake1;
    private readonly List<Snake> _snakeList = [];
    private readonly List<Player> _playerList = [];
    public IReadOnlyList<Player> PlayerList => _playerList;
    private Player Player1;
    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    //计时
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private long _lastUpdateMs = 0;
    private long _accumulatorMs = 0;

    public int SimStepMs { get; init; }

    private readonly int FoodSpawnIntervalMs;
    private int _foodSpawnCooldownMs;


    //config
    private readonly GameConfig Config;
    private readonly SkinConfig skinConfig;
    private readonly int FoodScore;


    //Constructor
    public Game(GameConfig config, SkinConfig skinConfig)
    {
        Config = config;
        this.skinConfig = skinConfig;
        SimStepMs = config.SimStepMs;
        FoodSpawnIntervalMs = config.FoodSpawnIntervalMs;
        FoodScore = config.FoodScore;
        _foodSpawnCooldownMs = FoodSpawnIntervalMs;

        State = GameState.Playing;
        snake1 = new(config, new(3, 0), Snake.Direction.Left);
        Player1 = new(01, snake1, skinConfig.Player1);
        _playerList.Add(Player1);
        _snakeList.Add(snake1);

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

    public void ToggleDirection_1(Snake.Direction direction)
    {
        Player1.snake.TryTurn(direction);
    }
    public void ToggleBoost_1()
    {
        Player1.snake.TryBoost();
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
            foreach (Snake snake in _snakeList)
            {
                snake.UpdateSnake();
                snake.Move();

            }
            CheckEatFood();
            SpawnFoodIfDue();
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
            Random.Shared.Next(2, Config.MapWidth - 2),
            Random.Shared.Next(2, Config.MapHeight - 2)
            );

            isOccupied = snake1.Body.Contains(newFood) || _foodList.Contains(newFood);
            if (!isOccupied)
            {
                _foodList.Add(newFood);
            }
        } while (isOccupied);
        _foodSpawnCooldownMs += FoodSpawnIntervalMs;
    }

    private void CheckCollision()//重点升级对象//
    {
        bool state_x = (snake1.Head.X <= 0 || snake1.Head.X >= Config.MapWidth - 1);
        bool state_y = (snake1.Head.Y <= 0 || snake1.Head.Y >= Config.MapHeight - 1);
        bool hitSelf = snake1.Body.Skip(1).Contains(snake1.Head);
        if (state_x || state_y || hitSelf)
        {
            State = GameState.GameOver;
        }
    }

    private void CheckEatFood()
    {
        foreach (Player player in _playerList)
        {
            bool isEatFood = _foodList.Contains(player.snake.Head);
            if (!isEatFood)
                continue;
            player.snake.Grow();
            var foodPosition = player.snake.Head;
            _foodList.Remove(foodPosition);
            player.GetPoint(FoodScore);
        }
    }
}

