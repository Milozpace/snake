using ProtoGameState = SnakeNetwork.Protocol.GameState;
using CoreGameState = Snake.Core.GameTypes.GameState;
using SnakeNetwork.Protocol;
using Snake.Core;
using GameSnake = Snake.Core.Snake;
using Snake.Core.Primitives;
using Snake.Config;
using Snake.Views;

namespace Snake.Network;

//通信模块 - 信息转换
public static class WorldSnapshotFactory
{
    public static WorldFrame ToWorldFrame(WorldSnapshot snapshot)
    {
        return new WorldFrame(
            Status: snapshot.GameState switch
            {
                ProtoGameState.Playing => CoreGameState.Playing,
                ProtoGameState.Paused => CoreGameState.Paused,
                ProtoGameState.GameOver => CoreGameState.GameOver,
                _ => throw new ArgumentOutOfRangeException(nameof(snapshot.GameState))
            },
            Players: [.. snapshot.Players.Select(ToPlayerFrame)],
            Foods: [.. snapshot.Foods.Select(point => new Point(point.X, point.Y))]
        );
    }

    private static PlayerFrame ToPlayerFrame(PlayerSnapshot snapshot)
    {
        return new PlayerFrame(
            Id: snapshot.PlayerId,
            Score: snapshot.Score,
            IsAlive: snapshot.IsAlive,
            Body: [.. snapshot.Body.Select(point => new Point(point.X, point.Y))],
            SkillState: snapshot.SkillStatus switch
            {
                SkillStatus.Ready => GameSnake.SkillState.Ready,
                SkillStatus.Active => GameSnake.SkillState.Active,
                SkillStatus.CoolingDown => GameSnake.SkillState.CoolingDown,
                _ => throw new ArgumentOutOfRangeException(nameof(snapshot.SkillStatus))
            },
            SkillActiveRemainingMs: snapshot.ActiveRemainingMs,
            SkillCooldownRemainingMs: snapshot.CooldownRemainingMs
        );
    }

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

    private static SkillStatus ToProtoSkillState(GameSnake.SkillState state)
    {
        return state switch
        {
            GameSnake.SkillState.Active => SkillStatus.Active,
            GameSnake.SkillState.Ready => SkillStatus.Ready,
            GameSnake.SkillState.CoolingDown => SkillStatus.CoolingDown,
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
