using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    private readonly List<Point> _body = [];
    public IReadOnlyList<Point> Body => _body;

    public Point Head => _body[0];
    private Point previousTailPosition;

    private readonly int InitMoveIntervalMs = 200;
    private int _moveIntervalMs;
    private int _moveCooldownMs;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;

    public int MoveCooldownMs { get; private set; }

    //共有数据
    private readonly int SimStepMs;
    public Snake(GameConfig config)
    {
        _body = [new(20, 10), new(19, 10), new(18, 10)];
        previousTailPosition = new Point(0, 0);

        MoveCooldownMs = config.InitMoveIntervalMs;
        _currentDirection = Direction.Right;
        _moveIntervalMs = InitMoveIntervalMs;
        _moveCooldownMs = InitMoveIntervalMs;

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

    //主要调用的操作方法
    public void Move()
    {
        _moveCooldownMs -= SimStepMs;//update 
        if (_moveCooldownMs > 0)
        {
            return;
        }

        previousTailPosition = _body[^1];
        Point newHead = CalculateNextHead();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);

        _moveCooldownMs += _moveIntervalMs;
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
        if (_moveIntervalMs > 120)
            _moveIntervalMs -= 3;
    }
}

public record Point(int X, int Y);