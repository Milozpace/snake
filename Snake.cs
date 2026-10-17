using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    public List<Point> body { get; private set; }
    public Point Snakehead;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public Direction currrentDirection;
    public Snake()
    {
        body = [new(20, 10), new(19, 10), new(18, 10)];
        Snakehead = new Point(body[0].X, body[0].Y);
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
        Point newHead = newhead();
        body.Insert(0, newHead);
        body.RemoveAt(body.Count - 1);
        Snakehead = new Point(body[0].X, body[0].Y);
    }

    public void grow()//待修改，需要在尾巴插入一个元素，但是如何保持方向？是x加减1/y加减1？
    {
        Point newHead = newhead();
        body.Insert(0, newHead);
    }
}

public record Point(int X, int Y);