using SnakeNetwork.Protocol;

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

    private static SkillStatus ToProtoSkillStatus(Snake.SkillState state)
    {
        return state switch
        {
            Snake.SkillState.Active => SkillStatus.Active,
            Snake.SkillState.Ready => SkillStatus.Ready,
            Snake.SkillState.CoolingDown => SkillStatus.CoolingDown,
            _ => SkillStatus.Unspecified
        };

    }

    private static GameState TransGameState(Game.GameState state)
    {
        return state switch
        {
            Game.GameState.Playing => GameState.Playing,
            Game.GameState.Paused => GameState.Pasued,
            Game.GameState.GameOver => GameState.Gameover,
            _ => GameState.Unspecified
        };
    }

    private static MapSize TransMap(GameConfig config)
    {
        return new MapSize
        {
            Width = config.MapWidth,
            Height = config.MapHeight
        };
    }


    //调用方法
    private static PlayerSnapshot CopyPlayer(Player player)
    {
        var snapshot = new PlayerSnapshot
        {
            PlayerId = player.Id,
            Score = player.Score,
            IsAlive = player.isAlive,
            SkillStatus = ToProtoSkillStatus(player.snake.CurrentSkillState),
            ActiveRemainingMs = player.snake._skillActiveTimerMs,
            CooldownRemainingMs = player.snake._skillCooldownTimerMs
        };
        snapshot.Body.Add(ToGridPoints(player.snake.Body));

        return snapshot;
    }

    public static WorldSnapshot FromGame(GameConfig config, Game game, ulong sequence)
    {
        var worldSnapshot = new WorldSnapshot()
        {
            Sequence = sequence,
            GameState = TransGameState(game.State),
            MapSize = TransMap(config)
        };

        foreach (Player player in game.PlayerList)
        {
            PlayerSnapshot playerSnapshot = CopyPlayer(player);
            worldSnapshot.Players.Add(playerSnapshot);
        }

        worldSnapshot.Foods.Add(ToGridPoints(game.FoodList));

        return worldSnapshot;
    }
}