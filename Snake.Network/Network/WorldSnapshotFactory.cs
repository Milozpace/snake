using ProtoGameState = SnakeNetwork.Protocol.GameState;
using CoreGameState = snake.Core.GameTypes.GameState;
using SnakeNetwork.Protocol;
using snake.Core;
using snake.Core.Primitives;
using snake.Config;

namespace snake.Network;

//通信模块 - 信息转换
public static class WorldSnapshotFactory
{
    //工具函数
    private static List<GridPoint> ToGridPoints(IReadOnlyList<Point> list)
    {
        List<GridPoint> result = new(list.Count);
        foreach (var food in list)
        {
            var protoPoint = new GridPoint
            {
                X = food.X,
                Y = food.Y
            };
            result.Add(protoPoint);
        }
        return result;
    }

    private static SkillStatus ToProtoSkillState(Snake.SkillState state)
    {
        return state switch
        {
            Snake.SkillState.Active => SkillStatus.Active,
            Snake.SkillState.Ready => SkillStatus.Ready,
            Snake.SkillState.CoolingDown => SkillStatus.CoolingDown,
            _ => SkillStatus.Unspecified
        };

    }

    private static ProtoGameState ToProtoGameState(CoreGameState state)
    {
        return state switch
        {
            CoreGameState.Playing => ProtoGameState.Playing,
            CoreGameState.Paused => ProtoGameState.Paused,
            CoreGameState.GameOver => ProtoGameState.GameOver,
            _ => ProtoGameState.Unspecified
        };
    }

    private static MapSize ToProtoMapsize(GameConfig config)
    {
        return new MapSize
        {
            Width = config.MapWidth,
            Height = config.MapHeight
        };
    }


    //调用方法
    private static PlayerSnapshot ToPlayerSnapshot(Player player)
    {
        var snapshot = new PlayerSnapshot
        {
            PlayerId = player.Id,
            Score = player.Score,
            IsAlive = player.IsAlive,
            SkillStatus = ToProtoSkillState(player.Snake.CurrentSkillState),
            ActiveRemainingMs = player.Snake.SkillActiveRemainingMs,
            CooldownRemainingMs = player.Snake.SkillCooldownRemainingMs
        };
        snapshot.Body.Add(ToGridPoints(player.Snake.Body));

        return snapshot;
    }

    public static WorldSnapshot ToWorldSnapshot(GameConfig config, Game game, ulong sequence)
    {
        var worldSnapshot = new WorldSnapshot()
        {
            Sequence = sequence,
            GameState = ToProtoGameState(game.GameState),
            MapSize = ToProtoMapsize(config)
        };

        foreach (Player player in game.PlayerList)
        {
            PlayerSnapshot playerSnapshot = ToPlayerSnapshot(player);
            worldSnapshot.Players.Add(playerSnapshot);
        }

        worldSnapshot.Foods.Add(ToGridPoints(game.FoodList));

        return worldSnapshot;
    }
}