public interface IGameView
{
    int MapWidth { get; }
    int MapHeight { get; }

    IReadOnlyList<IPlayerView> Players { get; }
    IReadOnlyList<Point> Foods { get; }
    Game.GameState GameStatus {get;}
}

public interface IPlayerView
{
    int Id { get; }
    int Score {get;}
    bool IsAlive {get;}
    IReadOnlyList<Point> Body { get; }

    Snake.SkillState SkillState { get; }
    int SkillActiveRemainingMs { get; }
    int SkillCooldownRemainingMs { get; }
}