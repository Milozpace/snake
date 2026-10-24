using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    private readonly List<Point> _body = [];
    public IReadOnlyList<Point> Body => _body;

    public Point Head => _body[0];
    private Point previousTailPosition;

    //Move
    private readonly int InitMoveIntervalMs;
    private readonly int BoostMoveIntervalMs;
    private int _baseMoveIntervalMs;

    private int _moveCooldownMs;

    private int CurrentMoveIntervalMs =>
       CurrentSkillState == SkillState.Active
           ? BoostMoveIntervalMs
           : _baseMoveIntervalMs;

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    //Skill
    public enum SkillState
    {
        CoolingDown,
        Ready,
        Active
    }

    public SkillState CurrentSkillState { get; private set; } = SkillState.Ready;
    private readonly int SkillCooldownMs;
    private readonly int SkillActiveMs;
    private int _skillCooldownTimerMs;
    private int _skillActiveTimerMs = 0;

    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;


    //共有数据
    private readonly int SimStepMs;
    public Snake(GameConfig config)
    {
        _body = [new(20, 10), new(19, 10), new(18, 10)];
        previousTailPosition = new Point(0, 0);

        InitMoveIntervalMs = 200;
        _currentDirection = Direction.Right;
        _baseMoveIntervalMs = InitMoveIntervalMs;
        BoostMoveIntervalMs = 100;
        SkillCooldownMs = 10000;
        SkillActiveMs = 5000;

        _moveCooldownMs = _baseMoveIntervalMs;
        SimStepMs = 5;
    }

    //私有工具方法
    private Point CalculateNextHead()
    {
        Point delta = _currentDirection switch
        {
            Direction.Up => new Point(0, -1),
            Direction.Down => new Point(0, 1),
            Direction.Left => new Point(-1, 0),
            Direction.Right => new Point(1, 0),
            _ => throw new ArgumentException()
        };

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

        previousTailPosition = _body[^1];
        Point newHead = CalculateNextHead();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);

        _moveCooldownMs += CurrentMoveIntervalMs;
    }

    public void TryTurn(Direction newDirection)
    {
        bool isOpposite = (_currentDirection, newDirection) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            _ => false
        };

        if (!isOpposite)
        {
            _currentDirection = newDirection;
        }
    }

    public void Grow()
    {
        _body.Add(previousTailPosition);

        _baseMoveIntervalMs = Math.Max(120, _baseMoveIntervalMs - 3);
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