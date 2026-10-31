public interface IGameView
{
    int MapWidth { get; }
    int MapHeight { get; }

    IReadOnlyList<IPlayerView> Players { get; }
    IReadOnlyList<Point> Foods { get; }
}

public interface IPlayerView
{
    int Id { get; }
    int Score {get;}
    IReadOnlyList<Point> Body { get; }
    SnakeSkin SnakeSkin {get;}
}