using System.Diagnostics;
using snake.Core.Primitives;
using snake.Core.GameTypes;

namespace snake.Core;

public class Game : IGameView
{
    //游戏状态
    public GameState State { get; private set; }
    public GameMode Mode { get; }
    public Player? Winner =>
        State == GameState.GameOver
            ? _playerList.FirstOrDefault(play => play.IsAlive)
            : null;

    //游戏成员
    private readonly List<Player> _playerList = [];//这个初始化是什么讲究
    public IReadOnlyList<Player> PlayerList => _playerList;

    private readonly List<Point> _foodList = [];
    public IReadOnlyList<Point> FoodList => _foodList;

    //interface：IGameView //所以说，这是配置，不应该作为接口放在这里吧？配置就直接用不就行了？
    int IGameView.MapWidth => Config.MapWidth;
    int IGameView.MapHeight => Config.MapHeight;
    IReadOnlyList<IPlayerView> IGameView.Players => _playerList;
    IReadOnlyList<Point> IGameView.Foods => _foodList;
    GameState IGameView.GameStatus => State;


    //计时
    //这或许是最大的混乱了……一定要有单独的计时模块
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private long _lastUpdateMs = 0;
    private long _accumulatorMs = 0;
    private const int MaxCatchUpSteps = 8;
    private int _foodSpawnCooldownMs;//依旧计时工具

    //config
    private readonly GameConfig Config;


    //Constructor
    public Game(GameConfig config, GameMode gameMode)
    {
        Mode = gameMode;
        Config = config;
        State = GameState.Playing;

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
    //对应input接口，会改变游戏的state和总游戏计时器
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

    //snake 的操作接口
    public void ToggleDirection(int playerId, Direction direction)
    {
        Player? player = _playerList.FirstOrDefault(p => p.Id == playerId);
        player?.Snake.TryTurn(direction);
    }
    public void ToggleBoost(int playerId)
    {
        Player? player = _playerList.FirstOrDefault(p => p.Id == playerId);
        player?.Snake.TryBoost();
    }

    /// <summary>
    /// 以上都是input接口，影响snake的状态和game的状态
    /// 其实还是做的不错的
    /// </summary>


    //游戏处理
    private void SpawnFoodIfDue() //更新间隔 -> 计算食物位置 -> 生成食物
    {
        _foodSpawnCooldownMs -= Config.SimStepMs;
        if (_foodSpawnCooldownMs > 0)
            return;

        List<Point> availablePositions = [];
        for (int y = 2; y < Config.MapHeight - 2; y++)
        {
            for (int x = 2; x < Config.MapWidth - 2; x++)
            {
                Point position = new(x, y);

                bool isOccupied = _playerList.Any(
                    player => player.Snake.Body.Contains(position) ||
                    _foodList.Contains(position)
                );

                if (!isOccupied)
                    availablePositions.Add(position);
            }
        }

        if (availablePositions.Count > 0)
        {
            int index = Random.Shared.Next(availablePositions.Count);
            _foodList.Add(availablePositions[index]);
        }

        _foodSpawnCooldownMs += Config.FoodSpawnIntervalMs;
    }

    private void CheckCollision(List<Player> movers) //检查碰撞，也就是裁判部分 //还是相当长的
    {
        foreach (Player player in movers)
        {
            Snake snake = player.Snake;
            bool hitWall = (snake.Head.X <= 0 ||
                snake.Head.X >= Config.MapWidth - 1) ||
                (snake.Head.Y <= 0 ||
                snake.Head.Y >= Config.MapHeight - 1);

            bool hitSelf = snake.Body.Skip(1).Contains(snake.Head);
            bool hitOther = _playerList.Any(otherPlayer =>
                otherPlayer.Snake != snake &&
                otherPlayer.Snake.Body.Contains(snake.Head)
            );
            if (hitWall || hitSelf || hitOther)
            {
                player.PlayerDie();
                Player? opponent = _playerList.FirstOrDefault(
                    other => other != player &&
                    other.Snake.Head == snake.Head &&
                    other.Snake.CurrentDirection == snake.CurrentDirection.Opposite
                );
                opponent?.PlayerDie();
            }
        }
        bool isGameOver = _playerList.Any(snakex => !snakex.IsAlive);
        if (isGameOver)
            State = GameState.GameOver;
    }

    private void CheckEatFood(List<Player> movers) //判断snake有没有吃到食物然后调用grow
    {
        foreach (Player player in movers)
        {
            bool isEatFood = _foodList.Contains(player.Snake.Head);
            if (!isEatFood)
                continue;
            player.Snake.Grow();
            var foodPosition = player.Snake.Head;
            _foodList.Remove(foodPosition);
            player.AddScore(Config.FoodScore);
        }
    }

    //处理游戏-接口
    public void UpdateGameIfdue()
    {
        if (State != GameState.Playing)
            return;

        //时间计算
        long now = _clock.ElapsedMilliseconds;
        _accumulatorMs += now - _lastUpdateMs;
        _lastUpdateMs = now;

        int step = 0;

        while (_accumulatorMs > Config.SimStepMs && step < MaxCatchUpSteps)
        {
            //更新玩家移动
            var movers = new List<Player>();
            foreach (Player player in _playerList)
            {
                Snake snake = player.Snake;
                snake.UpdateSnake();
                bool isMove = snake.Move();
                if (isMove)
                    movers.Add(player);
            }
            //检测机制（grow与die）
            if (movers.Count > 0)
            {
                CheckEatFood(movers);
                CheckCollision(movers);
            }
            //生成食物
            SpawnFoodIfDue();

            //更新时间
            _accumulatorMs -= Config.SimStepMs;
            step++;
        }
    }
}

//也就是说，在一个成员中分成多层有时候是没有必要的 —— 把这个角色完成的工作说清楚
//为什么写了那么长？ 
// 1.初始化的方式 2.计时模块 
// 3.裁判机制（死亡判断，主要是，的确是需要做这个事情，但是在这里写的太长了，但其实就是一个计算）
// 其他其实没什么的