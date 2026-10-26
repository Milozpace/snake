using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    private readonly List<Point> _body = [];
    public IReadOnlyList<Point> Body => _body;

    public Point Head => _body[0];
    private Point previousTailPosition = new(0, 0);

    //Move

    private int _baseMoveIntervalMs;

    private int _moveCooldownMs;

    private int CurrentMoveIntervalMs =>
       CurrentSkillState == SkillState.Active
           ? Math.Max(1, _baseMoveIntervalMs / 2)
           : _baseMoveIntervalMs;

    //Direction
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;
    private const int MaxBufferedTurns = 2;
    private readonly Queue<Direction> _pendingDIrection = new();

    //Grow
    private readonly int MinMoveIntervalMs;
    private readonly int MoveIntervalDecreaseMs;

    //Skill
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

        Point directionVector = GetDeltaPoint(initDirection);
        Point body_1 = new(head.X - directionVector.X, head.Y - directionVector.Y);
        Point body_2 = new(body_1.X - directionVector.X, body_1.Y - directionVector.Y);

        _body = [head,body_1, body_2];

        _currentDirection = initDirection;

        SimStepMs = config.SimStepMs;
        _baseMoveIntervalMs = config.InitMoveIntervalMs;
        MinMoveIntervalMs = config.MinMoveIntervalMs;
        MoveIntervalDecreaseMs = config.MoveIntervalDecreaseMs;
        SkillCooldownMs = config.SkillCooldownMs;
        SkillActiveMs = config.SkillActiveMs;

        _moveCooldownMs = _baseMoveIntervalMs;

    }

    //私有工具方法
    private Point GetDeltaPoint(Direction direction)
    {
        Point directionVector = direction switch
        {
            Direction.Up => new Point(0, -1),
            Direction.Down => new Point(0, 1),
            Direction.Left => new Point(-1, 0),
            Direction.Right => new Point(1, 0),
            _ => throw new ArgumentException()
        };
        return directionVector;
    }
    private Point CalculateNextHead()
    {
        Point delta = GetDeltaPoint(CurrentDirection);

        Point newHead = new Point(
            Head.X + delta.X,
            Head.Y + delta.Y
        );

        return newHead;
    }
    private void UpdateSkillState()
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

    private void UpdateDirection()
    {
        if (_pendingDIrection.Count() == 0)
            return;
        _currentDirection = _pendingDIrection.Dequeue();
    }

    //主要调用的操作方法
    public void UpdateSnake()
    {
        UpdateSkillState();
    }
    public void Move()
    {
        _moveCooldownMs -= SimStepMs;
        if (_moveCooldownMs > 0)
        {
            return;
        }

        UpdateDirection();
        previousTailPosition = _body[^1];
        Point newHead = CalculateNextHead();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);

        _moveCooldownMs += CurrentMoveIntervalMs;
    }

    public void TryTurn(Direction newDirection)
    {
        Direction lastDirection = _pendingDIrection.Count > 0
            ? _pendingDIrection.Last()
            : _currentDirection;

        bool isBannedDirection = (lastDirection, newDirection) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            (Direction.Up, Direction.Up) => true,
            (Direction.Down, Direction.Down) => true,
            (Direction.Left, Direction.Left) => true,
            (Direction.Right, Direction.Right) => true,
            _ => false
        };

        if (!isBannedDirection && _pendingDIrection.Count < MaxBufferedTurns)
        {
            _pendingDIrection.Enqueue(newDirection);
        }
    }

    public void Grow()
    {
        _body.Add(previousTailPosition);

        _baseMoveIntervalMs = Math.Max(MinMoveIntervalMs, _baseMoveIntervalMs - MoveIntervalDecreaseMs);
    }

    public bool TryBoost()
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

public record Point(int X, int Y);