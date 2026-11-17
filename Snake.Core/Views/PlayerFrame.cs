using System.Collections.Immutable;
using snake.Core;
using snake.Core.Primitives;

namespace snake.Views;

public sealed record PlayerFrame(
    int Id,
    int Score,
    bool IsAlive,
    ImmutableArray<Point> Body,
    Snake.SkillState SkillState,
    int SkillActiveRemainingMs,
    int SkillCooldownRemainingMs
);