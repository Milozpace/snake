namespace snake.Core.Primitives;

public record Point(int X, int Y)
{
    public static Point operator +(Point a, Point b)
        => new(a.X + b.X, a.Y + b.Y);

    public static Point operator -(Point a, Point b)
        => new(a.X - b.X, a.Y - b.Y);

    
}