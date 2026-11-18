using System.Collections.Immutable;
using Snake.Core.GameTypes;
using Snake.Core.Primitives;

namespace Snake.Views;

public sealed record WorldFrame(
    GameState Status,
    ImmutableArray<PlayerFrame> Players,
    ImmutableArray<Point> Foods
);