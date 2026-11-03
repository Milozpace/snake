using snake.Core;

public interface IGameView
{
    int MapWidth { get; }
    int MapHeight { get; }

    IReadOnlyList<IPlayerView> Players { get; }
    IReadOnlyList<Point> Foods { get; }
    Game.GameState GameStatus {get;}
}

public interface IPlayerView //所以，命名空间！ 以及，后续这个的接口？
{
    int Id { get; }
    int Score {get;}
    bool IsAlive {get;}
    IReadOnlyList<Point> Body { get; }

    Snake.SkillState SkillState { get; }
    int SkillActiveRemainingMs { get; }
    int SkillCooldownRemainingMs { get; }
}