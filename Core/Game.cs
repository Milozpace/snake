using System.Diagnostics;
using snake.Core.Primitives;
using snake.Core.GameTypes;
using snake.Views;
using snake.Config;

namespace snake.Core;

public class Game : IWorldView
{
    //游戏状态
    public GameState GameState { get; private set; }
    public GameMode Mode { get; }
    public Player? Winner =>
        GameState == GameState.GameOver
            ? _playerList.FirstOrDefault(play => play.IsAlive)
            : null;

    //游戏成员
    private readonly List<Player> _playerList = [];//这个初始化是什么讲究
    public IReadOnlyList<Player> PlayerList => _playerList;

    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    //interface：IGameView 
    // //所以说，这是配置，不应该作为接口放在这里吧？配置就直接用不就行了？
    int IWorldView.MapWidth => Config.MapWidth;
    int IWorldView.MapHeight => Config.MapHeight;
    IReadOnlyList<IPlayerView> IWorldView.Players => _playerList;
    IReadOnlyList<Point> IWorldView.Foods => _foodList;
    GameState IWorldView.GameStatus => GameState;


    //计时
    //这或许是最大的混乱了……一定要有单独的计时模块
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private long _now => _clock.ElapsedMilliseconds;

    private long _lastUpdateMs = 0;
    private long _accumulatorMs = 0;
    private const int MaxCatchUpSteps = 8;
    private int _foodSpawnCooldownMs;

    //config
    private readonly GameConfig Config;


    //Constructor
    public Game(GameConfig config, GameMode gameMode)
    {
        Mode = gameMode;
        Config = config;
        GameState = GameState.Playing;
        _foodSpawnCooldownMs = config.FoodSpawnIntervalMs;

        var snake1 = new Snake(config, new(-3, -1), Direction.Right);
        var Player1 = new Player(01, snake1);
        _playerList.Add(Player1);

        //将玩家2的初始化放到条件后面
        if (gameMode is not GameMode.SinglePlayer)
        {
            var snake2 = new Snake(config, new(3, 1), Direction.Left);
            var Player2 = new Player(02, snake2);
            _playerList.Add(Player2);
        }
    }

    //键盘输入接口
    public void TogglePause()
    {
        switch (GameState)
        {
            case GameState.Playing:
                {
                    GameState = GameState.Paused;
                    _clock.Stop();
                    break;
                }
            case GameState.Paused:
                {
                    GameState = GameState.Playing;
                    _clock.Start();
                    break;
                }
        }
    }

    public void ToggleDirection(int playerId, Direction direction)
    {
        Player? player = _playerList.FirstOrDefault(p => p.Id == playerId);
        player?.Snake.RequestTurn(direction);
    }
    public void ToggleBoost(int playerId)
    {
        Player? player = _playerList.FirstOrDefault(p => p.Id == playerId);
        player?.Snake.TryBoost();
    }


    //游戏更新
    private List<Point> FindAvailableFoodPotision()
    {
        List<Point> availablePositions = [];

        for (int y = 2; y < Config.MapHeight - 2; y++)
        {
            for (int x = 2; x < Config.MapWidth - 2; x++)
            {
                Point position = new(x, y);

                bool isOccupied =
                    _foodList.Contains(position) ||
                    _playerList.Any(player =>
                        player.Snake.Body.Contains(position));

                if (!isOccupied)
                    availablePositions.Add(position);
            }
        }

        return availablePositions;
    }

    private void SpawnFoodIfDue()
    {
        //更新时间
        _foodSpawnCooldownMs -= Config.SimStepMs;
        if (_foodSpawnCooldownMs > 0)
            return;

        List<Point> availablePositions = FindAvailableFoodPotision();
        if (availablePositions.Count > 0)
        {
            int index = Random.Shared.Next(availablePositions.Count);
            _foodList.Add(availablePositions[index]);
        }

        //时间处理
        _foodSpawnCooldownMs += Config.FoodSpawnIntervalMs;
    }

    private List<Player> AdvanceSnakes()
    {
        List<Player> movers = [];

        foreach (Player player in _playerList)
        {
            if (player.Snake.AdvanceStep())
                movers.Add(player);
        }

        return movers;
    }


    private void ResolveCollision(List<Player> movers)
    {
        foreach (Player player in movers)
        {
            Snake snake = player.Snake;
            bool isHitBounds = (snake.Head.X <= 0 ||
                snake.Head.X >= Config.MapWidth - 1) ||
                (snake.Head.Y <= 0 ||
                snake.Head.Y >= Config.MapHeight - 1);

            bool isHitSelf = snake.Body
                .Skip(1)
                .Contains(snake.Head);

            bool isHitOthers = _playerList.Any(otherPlayer =>
                otherPlayer.Snake != snake &&
                otherPlayer.Snake.Body.Contains(snake.Head)
            );
            if (isHitBounds || isHitSelf || isHitOthers)
            {
                player.Die();
                Player? opponent = _playerList.FirstOrDefault(
                    other => other != player &&
                    other.Snake.Head == snake.Head &&
                    other.Snake.CurrentDirection == snake.CurrentDirection.Opposite
                );
                opponent?.Die();
                //一个反向头撞头的特殊情况判定（两条蛇都会死亡）
            }
        }

        //处理游戏状态
        bool isGameOver = _playerList.Any(snakex => !snakex.IsAlive);
        if (isGameOver)
            GameState = GameState.GameOver;
    }

    private void ResolveFoodConsumption(List<Player> movers)
    {
        foreach (Player player in movers)
        {
            bool hasEatenFood = _foodList.Contains(player.Snake.Head);

            if (hasEatenFood)
            {
                player.Snake.Grow();
                player.AddScore(Config.FoodScore);

                var eatenFoodPosition = player.Snake.Head;
                _foodList.Remove(eatenFoodPosition);
            }
        }
    }

    private void SimulateStep()
    {
        var movers = AdvanceSnakes();

        if (movers.Count > 0)
        {
            ResolveFoodConsumption(movers);
            ResolveCollision(movers);
        }
        SpawnFoodIfDue();
    }


    public void UpdateGameIfdue()
    {
        _accumulatorMs += _now - _lastUpdateMs;
        _lastUpdateMs = _now;

        if (_accumulatorMs < Config.SimStepMs)
            return;

        int step = 0;
        while (_accumulatorMs >= Config.SimStepMs && step < MaxCatchUpSteps)
        {
            SimulateStep();
            _accumulatorMs -= Config.SimStepMs;
            step++;
        }
    }
}

