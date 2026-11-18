using System.Collections.Immutable;
using GameSnake = Snake.Core.Snake;
using Snake.Core.Primitives;

namespace Snake.Views;

public sealed record PlayerFrame(
    int Id,
    int Score,
    bool IsAlive,
    ImmutableArray<Point> Body,
    GameSnake.SkillState SkillState,
    int SkillActiveRemainingMs,
    int SkillCooldownRemainingMs
);