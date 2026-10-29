using SnakeNetwork.Protocol;
using ProtoDirection = SnakeNetwork.Protocol.Direction;

public static class RemoteInputProcessor
{
    //其实似乎和本地键盘输入的职责是一样的呢
    //guest不允许暂停和退出（游戏）
    //上下左右玩家id（只允许player2） 加速
    public static void Apply(Game game, PlayerInput input)
    {
        int id = 2;
        ProtoDirection inputDirection = input.Direction;
        bool useBoost = input.Boost;

        Snake.Direction? newDirection = inputDirection switch
        {
            ProtoDirection.Up => Snake.Direction.Up,
            ProtoDirection.Down => Snake.Direction.Down,
            ProtoDirection.Left => Snake.Direction.Left,
            ProtoDirection.Right => Snake.Direction.Right,
            _ => null
        };

        if (newDirection is { } validDirection)
            game.ToggleDirection(id, validDirection);
        
        if (useBoost)
            game.ToggleBoost(id);
    }
}