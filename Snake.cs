using System.Reflection;
using System.Runtime.CompilerServices;
public class Snake
{
    public List<Point> body { get; private set; }
    public Point Snakehead;
    public Point SnakeTail;
    public Point Snakeoldtail;
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
        SnakeTail = new Point(body[body.Count - 1].X, body[body.Count - 1].Y);
        Snakeoldtail = new Point(0, 0);

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
    public bool Eatfood(Point head, List<Point> foodlist)
    {
        if (foodlist.Contains(head))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void move(List<Point> foodlist)
    {
        Snakeoldtail = new Point(body[body.Count - 1].X, body[body.Count - 1].Y);
        Point newHead = newhead();
        body.Insert(0, newHead);
        body.RemoveAt(body.Count - 1);

        Snakehead = new Point(body[0].X, body[0].Y);
        SnakeTail = new Point(body[body.Count - 1].X, body[body.Count - 1].Y);
    }

    public void grow()
    {
        body.Add(Snakeoldtail);
    }
}


public record Point(int X, int Y);