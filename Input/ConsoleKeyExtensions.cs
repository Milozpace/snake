using snake.Core.Primitives;

namespace snake.Input;

public static class ConsoleKeyExtensions
{
    extension(ConsoleKey key)
    {
        public Direction? ToGameDirection() => key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => Direction.Up,
            ConsoleKey.DownArrow or ConsoleKey.S => Direction.Down,
            ConsoleKey.LeftArrow or ConsoleKey.A => Direction.Left,
            ConsoleKey.RightArrow or ConsoleKey.D => Direction.Right,
            _ => null
        };
    }
}