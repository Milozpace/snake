using ProtoDirection = SnakeNetwork.Protocol.Direction;
using GameDirection = snake.Core.Primitives.Direction;

namespace snake.Input;

public static class ConsoleKeyExtensions
{
    extension(ConsoleKey key)
    {
        public GameDirection? ToGameDirectionFromWasdKeys() => key switch
        {
            ConsoleKey.W => GameDirection.Up,
            ConsoleKey.S => GameDirection.Down,
            ConsoleKey.A => GameDirection.Left,
            ConsoleKey.D => GameDirection.Right,
            _ => null
        };

        public GameDirection? ToGameDirectionFromArrowKeys() => key switch
        {
            ConsoleKey.UpArrow => GameDirection.Up,
            ConsoleKey.DownArrow => GameDirection.Down,
            ConsoleKey.LeftArrow => GameDirection.Left,
            ConsoleKey.RightArrow => GameDirection.Right,
            _ => null
        };

        public ProtoDirection ToProtoDirection() => key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => ProtoDirection.Up,
            ConsoleKey.DownArrow or ConsoleKey.S => ProtoDirection.Down,
            ConsoleKey.LeftArrow or ConsoleKey.A => ProtoDirection.Left,
            ConsoleKey.RightArrow or ConsoleKey.D => ProtoDirection.Right,
            _ => ProtoDirection.Unspecified
        };
    }
}