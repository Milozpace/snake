using System.Collections.Immutable;
using snake.Core.GameTypes;
using snake.Core.Primitives;

namespace snake.Views;

public sealed record WorldFrame(
    GameState Status,
    ImmutableArray<PlayerFrame> Players,
    ImmutableArray<Point> Foods
);