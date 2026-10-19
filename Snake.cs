using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    private readonly List<Point> body = new();
    public IReadOnlyList<Point> Body => body;

    public Point snake_head => body[0];
    public Point snake_tail => body[^1];
    private Point snake_oldtail;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private Direction currrentDirection;
    public Direction CurrrentDirection => currrentDirection;
    public Snake()
    {
        body = [new(20, 10), new(19, 10), new(18, 10)];
        snake_oldtail = new Point(0, 0);

        this.currrentDirection = Direction.Right;
    }

    public double speed { get; private set; }

    private Point newhead()
    {
        Point delta = currrentDirection switch
        {
            Direction.Up => new Point(0, -1),
            Direction.Down => new Point(0, 1),
            Direction.Left => new Point(-1, 0),
            Direction.Right => new Point(1, 0),
            _ => throw new ArgumentException()
        };

        Point Head = body[0];
        Point newHead = new Point(
            Head.X + delta.X,
            Head.Y + delta.Y
        );

        return newHead;
    }
    public void move()
    {
        snake_oldtail = body[^1];
        Point newHead = newhead();
        body.Insert(0, newHead);
        body.RemoveAt(body.Count - 1);
    }

    public void Turn(Direction newDirection)
    {
        bool isOpposite = (currrentDirection, newDirection) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            _ => false
        };

        if(!isOpposite)
        {
            currrentDirection = newDirection;
        }
    }

    public void grow()
    {
        body.Add(snake_oldtail);
    }
}


public record Point(int X, int Y);