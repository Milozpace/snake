namespace Snake.Views;

public sealed record WorldDefinition(
    int MapWidth,
    int MapHeight,
    int SkillActiveMs,
    int SkillCooldownMs
);