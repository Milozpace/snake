using snake.Core.Primitives;

namespace snake.Input;

public static class ConsoleKeyExtensions
{
    extension(ConsoleKey key)
    {
        public Direction? ToDirectionFromWasdKeys() => key switch
        {
            ConsoleKey.W => Direction.Up,
            ConsoleKey.S => Direction.Down,
            ConsoleKey.A => Direction.Left,
            ConsoleKey.D => Direction.Right,
            _ => null
        };

        public Direction? ToDirectionFromArrowKeys() => key switch
        {
            ConsoleKey.UpArrow => Direction.Up,
            ConsoleKey.DownArrow => Direction.Down,
            ConsoleKey.LeftArrow => Direction.Left,
            ConsoleKey.RightArrow => Direction.Right,
            _ => null
        };
    }
}