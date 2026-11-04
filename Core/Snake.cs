using snake.Core.Primitives;

namespace snake.Core;

public class Snake
{
    //蛇的身体
    private readonly List<Point> _body = [];
    public IReadOnlyList<Point> Body => _body;
    public Point Head => _body[0];
    private Point _previousTailPosition = new(0, 0);

    //移动时间管理
    private int _baseMoveIntervalMs;
    private int _moveRemainingMs;

    private int CurrentMoveIntervalMs =>
       CurrentSkillState == SkillState.Active
           ? Math.Max(1, _baseMoveIntervalMs / 2)
           : _baseMoveIntervalMs;

    //Direction
    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;
    private const int MaxBufferedTurns = 2;
    private readonly Queue<Direction> _pendingDirections = new();


    //Skill：这里面是skill，但是skill不应该是snake的一个模块吗？
    public enum SkillState
    {
        CoolingDown,
        Ready,
        Active
    }

    public SkillState CurrentSkillState { get; private set; } = SkillState.Ready;

    public int SkillCooldownRemainingMs { get; private set; }//设成public，因为renderer需要读取来绘制技能条
    public int SkillActiveRemainingMs { get; private set; }


    //config
    private readonly GameConfig _config;

    //Constructor
    public Snake(GameConfig config, Point offsetFromCenter, Direction initDirection)
    {
        _config = config;

        Point head = new(config.MapWidth / 2 + offsetFromCenter.X, config.MapHeight / 2 + offsetFromCenter.Y);
        Point delta = initDirection.Delta;
        _body =[
            head,
            head - delta,
            head - delta - delta
        ];

        _currentDirection = initDirection;
        _baseMoveIntervalMs = config.InitMoveIntervalMs;
        _moveRemainingMs = _baseMoveIntervalMs;
    }


    private Point CalculateNextHeadPosition()
        => Head + CurrentDirection.Delta;

    private void UpdateSkillState() 
    {
        switch (CurrentSkillState)
        {
            case SkillState.Ready:
                return;

            case SkillState.Active:
                {
                    SkillActiveRemainingMs = Math.Max(0, SkillActiveRemainingMs - _config.SimStepMs);
                    if (SkillActiveRemainingMs == 0)
                    {
                        CurrentSkillState = SkillState.CoolingDown;
                        SkillCooldownRemainingMs = _config.SkillCooldownMs;
                    }
                    break;
                }

            case SkillState.CoolingDown:
                {
                    SkillCooldownRemainingMs = Math.Max(0, SkillCooldownRemainingMs - _config.SimStepMs);
                    if (SkillCooldownRemainingMs == 0)
                        CurrentSkillState = SkillState.Ready;

                    break;
                }
        }
    }

    private void ApplyPendingTurn()
    {
        if (_pendingDirections.Count() == 0)
            return;
        _currentDirection = _pendingDirections.Dequeue();
    }

    //主要调用的操作方法
    public bool AdvanceStep()//什么updateupdate的是不是太模糊了？更新snake是什么，更新什么？
    {
        UpdateSkillState();
        _moveRemainingMs -= _config.SimStepMs;
        return TryMove();
    }

    private bool TryMove()
    {
        if (_moveRemainingMs > 0)
            return false;

        ApplyPendingTurn();
        //处理body更新
        _previousTailPosition = _body[^1];
        Point newHead = CalculateNextHeadPosition();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);

        _moveRemainingMs += CurrentMoveIntervalMs;
        return true;
    }

    public void RequestTurn(Direction newDirection)
    {
        if (_pendingDirections.Count >= MaxBufferedTurns)
            return;

        Direction lastDirection = _pendingDirections.Count > 0
            ? _pendingDirections.Last()
            : _currentDirection;

        if (newDirection == lastDirection ||
            newDirection == lastDirection.Opposite)
            return;

        _pendingDirections.Enqueue(newDirection);
    }

    public void Grow()
    {
        _body.Add(_previousTailPosition);

        _baseMoveIntervalMs = Math.Max(
            _config.MinMoveIntervalMs,
            _baseMoveIntervalMs - _config.MoveIntervalDecreaseMs
        );
    }

    public bool TryBoost()
    {
        if (CurrentSkillState == SkillState.Ready)
        {
            CurrentSkillState = SkillState.Active;
            SkillActiveRemainingMs = _config.SkillActiveMs;
            _moveRemainingMs = Math.Max(0, Math.Min(_moveRemainingMs, CurrentMoveIntervalMs));
            return true;
        }
        else
            return false;
    }
}

