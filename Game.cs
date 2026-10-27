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
    public GameRunner.GameMode gameMode { get; init; }
    public Player? Winner =>
        State == GameState.GameOver
            ? _playerList.FirstOrDefault(play => play.isAlive)
            : null;

    //游戏成员

    private Snake snake1;
    private Snake snake2;
    private readonly List<Snake> _snakeList = [];//没有必要保留两个访问入口，只保留playerlist
    private readonly List<Player> _playerList = [];
    public IReadOnlyList<Player> PlayerList => _playerList;
    private Player Player1;
    private Player Player2;
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
    public Game(GameConfig config, SkinConfig skinConfig, GameRunner.GameMode gameMode)
    {
        this.gameMode = gameMode;
        Config = config;
        this.skinConfig = skinConfig;
        SimStepMs = config.SimStepMs;
        FoodSpawnIntervalMs = config.FoodSpawnIntervalMs;
        FoodScore = config.FoodScore;
        _foodSpawnCooldownMs = FoodSpawnIntervalMs;

        State = GameState.Playing;
        snake1 = new(config, new(-3, -1), Snake.Direction.Right);
        snake2 = new(config, new(3, 1), Snake.Direction.Left);
        Player1 = new(01, snake1, skinConfig.Player1);
        Player2 = new(02, snake2, skinConfig.Player2);

        _playerList.Add(Player1);
        _playerList.Add(Player2);
        _snakeList.Add(snake1);
        _snakeList.Add(snake2);
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
    public void ToggleDirection_2(Snake.Direction direction)
    {
        Player2.snake.TryTurn(direction);
    }
    public void ToggleBoost_2()
    {
        Player2.snake.TryBoost();
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
            var movers = new List<Player>();
            foreach (Player player in _playerList)
            {
                Snake snake = player.snake;
                snake.UpdateSnake();
                bool isMove = snake.Move();
                if (isMove)
                    movers.Add(player);
            }
            if (movers.Count > 0)
            {
                CheckEatFood(movers);
                CheckCollision(movers);
            }
            SpawnFoodIfDue();

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

            isOccupied = _snakeList.Any(
                snake => snake.Body.Contains(newFood)) ||
                _foodList.Contains(newFood);
            if (!isOccupied)
            {
                _foodList.Add(newFood);
            }
        } while (isOccupied);
        _foodSpawnCooldownMs += FoodSpawnIntervalMs;
    }

    private void CheckCollision(List<Player> movers)
    {
        foreach (Player player in movers)
        {
            Snake snake = player.snake;
            bool hitWall = (snake.Head.X <= 0 ||
                snake.Head.X >= Config.MapWidth - 1) ||
                (snake.Head.Y <= 0 ||
                snake.Head.Y >= Config.MapHeight - 1);

            bool hitSelf = snake.Body.Skip(1).Contains(snake.Head);
            bool hitOther = _snakeList.Any(other =>
                other != snake &&
                other.Body.Contains(snake.Head)
            );
            if (hitWall || hitSelf || hitOther)
            {
                player.PlayerDie();
                Player? opponent = _playerList.FirstOrDefault(
                    other => other != player &&
                    other.snake.Head == snake.Head &&
                    other.snake.CurrentDirection == player.snake.GetOppositeDirection()
                );
                opponent?.PlayerDie();
            }
            bool isGameOver = _playerList.Any(snakex => !snakex.isAlive);
            if (isGameOver)
                State = GameState.GameOver;
        }
    }

    private void CheckEatFood(List<Player> movers)
    {
        foreach (Player player in movers)
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

