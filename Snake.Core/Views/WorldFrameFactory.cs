using System.Collections.Immutable;
using snake.Core;
using snake.Core.GameTypes;
using snake.Core.Primitives;

namespace snake.Views;

public static class WorldFrameFactory
{
    private static PlayerFrame CreatePlayerFrame(Player player)
    {
        return new PlayerFrame(
            Id: player.Id,
            Score: player.Score,
            IsAlive: player.IsAlive,
            Body: [.. player.Snake.Body],
            SkillState: player.SkillState,
            SkillActiveRemainingMs: player.SkillActiveRemainingMs,
            SkillCooldownRemainingMs: player.SkillCooldownRemainingMs
        );
    }

    public static WorldFrame CreateWorldFrame(Game game)
    {
        return new WorldFrame(
            Status: game.GameState,
            Players: [.. game.PlayerList.Select(CreatePlayerFrame)],
            Foods: [.. game.FoodList]
        );
    }
}