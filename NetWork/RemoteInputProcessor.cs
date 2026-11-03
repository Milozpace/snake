using SnakeNetwork.Protocol;
using ProtoDirection = SnakeNetwork.Protocol.Direction;
using snake.Core;
using GameDirection = snake.Core.Primitives.Direction;

public static class RemoteInputProcessor
{
    //Host的一个处理器
    //收到PlayerInput 之后会调用Game的对应接口（加速、方向移动）
    public static void Apply(Game game, PlayerInput input)
    {
        int id = 2;
        ProtoDirection inputDirection = input.Direction;
        bool useBoost = input.Boost;

        GameDirection? newDirection = inputDirection switch
        {
            ProtoDirection.Up => GameDirection.Up,
            ProtoDirection.Down => GameDirection.Down,
            ProtoDirection.Left => GameDirection.Left,
            ProtoDirection.Right => GameDirection.Right,
            _ => null
        };

        if (newDirection is { } validDirection)
            game.ToggleDirection(id, validDirection);
        
        if (useBoost)
            game.ToggleBoost(id);
    }
}