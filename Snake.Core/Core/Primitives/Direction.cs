namespace snake.Core.Primitives;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    extension(Direction dir)
    {
        public Point Delta => dir switch
        {
            Direction.Up => new(0, -1),
            Direction.Down => new(0, 1),
            Direction.Left => new(-1, 0),
            Direction.Right => new(1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(dir))
        };

        public Direction Opposite => dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(dir))
        };
    }
}