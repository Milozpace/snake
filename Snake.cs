using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    private readonly List<Point> _body = new();
    public IReadOnlyList<Point> Body => _body;

    public Point Head => _body[0];
    public Point Tail => _body[^1];
    private Point previousTailPosition;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private Direction _currentDirection;
    public Direction CurrentDirection => _currentDirection;
    public Snake()
    {
        _body = [new(20, 10), new(19, 10), new(18, 10)];
        previousTailPosition = new Point(0, 0);

        _currentDirection = Direction.Right;
    }

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
    public void Move()
    {
        previousTailPosition = _body[^1];
        Point newHead = CalculateNextHead();
        _body.Insert(0, newHead);
        _body.RemoveAt(_body.Count - 1);
    }

    public void Turn(Direction newDirection)
    {
        bool isOpposite = (_currentDirection, newDirection) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            _ => false
        };

        if(!isOpposite)
        {
            _currentDirection = newDirection;
        }
    }

    public void Grow()
    {
        _body.Add(previousTailPosition);
    }
}


public record Point(int X, int Y);