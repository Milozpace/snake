using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using snake.Core.Primitives;

namespace snake.Core;

public class Snake
{
    //蛇的身体
    private readonly List<Point> _body = [];
    public IReadOnlyList<Point> Body => _body;
    public Point Head => _body[0];
    private Point previousTailPosition = new(0, 0);

    //移动时间管理
    private int _baseMoveIntervalMs;
    private int _moveCooldownMs;

    private int CurrentMoveIntervalMs =>
       CurrentSkillState == SkillState.Active
           ? Math.Max(1, _baseMoveIntervalMs / 2)
           : _baseMoveIntervalMs;

    //Direction
    //（就像之前在gamemode做的那样）
    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;
    private const int MaxBufferedTurns = 2;
    private readonly Queue<Direction> _pendingDIrection = new();

    //Grow（相关的一些grow配置数字，用来管理移动时的移动速度处理，可以保留）
    private readonly int MinMoveIntervalMs;
    private readonly int MoveIntervalDecreaseMs;

    //Skill：这里面是skill，但是skill不应该是snake的一个模块吗？
    public enum SkillState
    {
        CoolingDown,
        Ready,
        Active
    }

    public SkillState CurrentSkillState { get; private set; } = SkillState.Ready;
    public readonly int SkillCooldownMs;
    public readonly int SkillActiveMs;
    public int _skillCooldownTimerMs { get; private set; }
    public int _skillActiveTimerMs { get; private set; } = 0;

    //共有数据
    private readonly int SimStepMs;

    //Constructor
    public Snake(GameConfig config, Point offsetFromCenter, Direction initDirection)
    {
        Point head = new(config.MapWidth / 2 + offsetFromCenter.X, config.MapHeight / 2 + offsetFromCenter.Y);

        Point delta = DirectionExtensions.get_Delta(initDirection);
        Point body_1 = new(head.X - delta.X, head.Y - delta.Y);
        Point body_2 = new(body_1.X - delta.X, body_1.Y - delta.Y); 
        //其实这里的配置就耦合了？
        //这个初始的写的有点长了（次要问题）

        _body = [head, body_1, body_2];

        _currentDirection = initDirection;

        SimStepMs = config.SimStepMs;
        _baseMoveIntervalMs = config.InitMoveIntervalMs;
        MinMoveIntervalMs = config.MinMoveIntervalMs;
        MoveIntervalDecreaseMs = config.MoveIntervalDecreaseMs;
        SkillCooldownMs = config.SkillCooldownMs;
        SkillActiveMs = config.SkillActiveMs;

        _moveCooldownMs = _baseMoveIntervalMs;

    }


    private Point CalculateNextHead()//调用了上一个函数，けっきょく，完成的任务就是一个事情嘛
    {
        Point delta = CurrentDirection.Delta;

        Point newHead = new(
            Head.X + delta.X,
            Head.Y + delta.Y
        );

        return newHead;
    }
    private void UpdateSkillState() 
    //更新技能状态（重要！）（但是可以简化？）（属于snake的状态更新之一）
    //但是这些any更新或许都应该放在一起
    //然而现在的计时器已经遍野都是

    {
        if (CurrentSkillState == SkillState.Ready)
            return;

        if (CurrentSkillState == SkillState.CoolingDown)
        {
            _skillCooldownTimerMs = Math.Max(0, _skillCooldownTimerMs - SimStepMs);
            if (_skillCooldownTimerMs == 0)
                CurrentSkillState = SkillState.Ready;
        }
        else if (CurrentSkillState == SkillState.Active)
        {
            _skillActiveTimerMs = Math.Max(0, _skillActiveTimerMs - SimStepMs);
            if (_skillActiveTimerMs == 0)
            {
                CurrentSkillState = SkillState.CoolingDown;
                _skillCooldownTimerMs = SkillCooldownMs;
            }
        }
    }

    private void UpdateDirection()//作为turn的第一步（所以应该放在一起？）
    {
        if (_pendingDIrection.Count() == 0)
            return;
        _currentDirection = _pendingDIrection.Dequeue();
    }

    //主要调用的操作方法
    public void UpdateSnake()//什么updateupdate的是不是太模糊了？更新snake是什么，更新什么？
    {
        UpdateSkillState();
    }

    public bool Move() //这是move的核心方法，
    {
        _moveCooldownMs -= SimStepMs;
        if (_moveCooldownMs > 0)
            return false;

        UpdateDirection();//ここだよ
        previousTailPosition = _body[^1];
        Point newHead = CalculateNextHead();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);//掐头去尾

        _moveCooldownMs += CurrentMoveIntervalMs;
        return true;
    }

    public void TryTurn(Direction newDirection)//try也好奇怪，是不是应该叫请求？
    {
        if (_pendingDIrection.Count >= MaxBufferedTurns)
            return;

        Direction lastDirection = _pendingDIrection.Count > 0
            ? _pendingDIrection.Last()
            : _currentDirection;

        if (newDirection == lastDirection ||
            newDirection == lastDirection.Opposite)
            return;

        _pendingDIrection.Enqueue(newDirection); 
    }

    public void Grow()
    {
        _body.Add(previousTailPosition);

        _baseMoveIntervalMs = Math.Max(MinMoveIntervalMs, _baseMoveIntervalMs - MoveIntervalDecreaseMs);
    }

    public bool TryBoost()//命名
    {
        if (CurrentSkillState == SkillState.Ready)
        {
            CurrentSkillState = SkillState.Active;
            _skillActiveTimerMs = SkillActiveMs;
            _moveCooldownMs = Math.Max(0, Math.Min(_moveCooldownMs, CurrentMoveIntervalMs));
            return true;
        }
        else
            return false;
    }
}

