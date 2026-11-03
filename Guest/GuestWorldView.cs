using SnakeNetwork.Protocol;
using snake.Core;
using snake.Core.Primitives;

public class GuestWorldView : IGameView
{
    private readonly WorldSnapshot _snapshot;

    public int MapWidth => _snapshot.MapSize.Width;
    public int MapHeight => _snapshot.MapSize.Height;

    public IReadOnlyList<IPlayerView> Players { get; }
    public IReadOnlyList<Point> Foods { get; }
    public Game.GameState GameStatus { get; }

    public GuestWorldView(WorldSnapshot snapshot)
    {
        _snapshot = snapshot;
        
        Players = snapshot.Players
            .Select(playerSnapshot => new GuestPlayerView(playerSnapshot))
            .ToArray();

        Foods = snapshot.Foods
            .Select(p => new Point(p.X, p.Y))
            .ToArray();

        GameStatus = snapshot.GameState switch
        {
            GameState.Playing => Game.GameState.Playing,
            GameState.Paused => Game.GameState.Paused,
            GameState.GameOver => Game.GameState.GameOver,
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot.GameState))
        };
    }

}
