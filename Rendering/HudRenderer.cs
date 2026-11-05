
namespace snake.Rendering;

using snake.Config;
using snake.Core;
using snake.Views;

public class HudRenderer(GameConfig config)
{
    private readonly GameConfig _config = config;



    // 玩家信息起始位置
    private static int GetPlayerInfoStartRow(IWorldView game) =>
        game.Players.Count == 1 ? -1 : -2;

    // 绘制玩家分数
    private void DrawPlayerInfo(
        CanvasPainter canvas,
        IWorldView game)
    {
        int startRow = GetPlayerInfoStartRow(game);

        for (int i = 0; i < game.Players.Count; i++)
        {
            IPlayerView player = game.Players[i];

            ConsoleColor playerColor = player.Id == 1
                ? ConsoleColor.Green
                : ConsoleColor.Cyan;

            canvas.DrawMapRichText(
                1,
                startRow + i,
                (playerColor, $"P{player.Id}  "),
                (ConsoleColor.DarkGray, "SCORE "),
                (ConsoleColor.White, $"{player.Score:D3}")
            );
        }
    }

    // 绘制所有玩家技能条
    public void DrawPlayerSkillbars(
        CanvasPainter canvas,
        IWorldView game)
    {
        int startRow = GetPlayerInfoStartRow(game);

        for (int i = 0; i < game.Players.Count; i++)
        {
            DrawSkillbar(
                canvas,
                game,
                game.Players[i],
                startRow + i);
        }
    }

    // 绘制标题和玩家信息
    public void DrawHeader(
        CanvasPainter canvas,
        IWorldView game)
    {
        int titleHeight = game.Players.Count == 2
            ? -3
            : -2;

        canvas.DrawCenteredMapText(
            titleHeight,
            ConsoleColor.DarkGreen,
            "◆  S N A K E  ◆");

        DrawPlayerInfo(canvas, game);
    }

    // 绘制操作提示
    public void DrawFooter(
        CanvasPainter canvas,
        IWorldView game)
    {
        canvas.DrawMapText(
            1,
            _config.MapHeight,
            ConsoleColor.DarkGreen,
            "◦ ↑←↓→ + M  BOOST");

        canvas.DrawMapText(
            _config.MapWidth - 5,
            _config.MapHeight,
            ConsoleColor.DarkGray,
            "ESC QUIT");

        if (game.Players.Count > 1)
        {
            canvas.DrawMapText(
                11,
                _config.MapHeight,
                ConsoleColor.DarkCyan,
                "◦ WASD + Q  BOOST");
        }
    }

    // 技能条显示信息
    private void DrawSkillbar(
        CanvasPainter canvas,
        IWorldView game,
        IPlayerView player,
        int mapRow)
    {
        ConsoleColor playerColor = player.Id == 1
            ? ConsoleColor.Green
            : ConsoleColor.Cyan;

        string key = player.Id == 1 ? "M" : "Q";

        double progress;
        string label;
        ConsoleColor fillColor;
        ConsoleColor textColor;

        switch (player.SkillState)
        {
            case Snake.SkillState.Ready:
                progress = 1.0;
                label = $"READY [{key}]";
                fillColor = playerColor;
                textColor = playerColor;
                break;

            case Snake.SkillState.Active:
                progress =
                    (double)player.SkillActiveRemainingMs /
                    _config.SkillActiveMs;

                double seconds =
                    player.SkillActiveRemainingMs / 1000.0;

                label = $"{seconds:0.0}s";
                fillColor = ConsoleColor.Yellow;
                textColor = ConsoleColor.Yellow;
                break;

            case Snake.SkillState.CoolingDown:
                progress =
                    1.0 -
                    (double)player.SkillCooldownRemainingMs /
                    _config.SkillCooldownMs;

                label = FormatCooldown(progress);

                fillColor = progress >= 0.80
                    ? playerColor
                    : ConsoleColor.Gray;

                textColor = fillColor;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(player.SkillState));
        }

        DrawSkillMeter(
            canvas,
            game,
            progress,
            label,
            playerColor,
            fillColor,
            textColor,
            mapRow);
    }

    // 技能条绘制
    private void DrawSkillMeter(
        CanvasPainter canvas,
        IWorldView game,
        double progress,
        string label,
        ConsoleColor playerColor,
        ConsoleColor fillColor,
        ConsoleColor textColor,
        int mapRow)
    {
        const int labelWidth = 9;

        int mapWidthChars = game.MapWidth * 2;
        int reservedWidth = 7 + 2 + labelWidth;

        int maxBarWidth = Math.Max(
            1,
            mapWidthChars - reservedWidth);

        int barWidth = Math.Clamp(
            game.MapWidth / 4,
            6,
            12);

        barWidth = Math.Min(barWidth, maxBarWidth);

        progress = Math.Clamp(progress, 0.0, 1.0);

        int filled = (int)Math.Floor(progress * barWidth);

        if (progress > 0 && filled == 0)
            filled = 1;

        filled = Math.Clamp(filled, 0, barWidth);

        int blockWidth = 7 + barWidth + 2 + labelWidth;

        int x = Math.Max(
            0,
            mapWidthChars - blockWidth);

        canvas.DrawMapCharacterText(
            x,
            mapRow,
            playerColor,
            "BOOST [");

        x += 7;

        canvas.DrawMapCharacterText(
            x,
            mapRow,
            fillColor,
            new string('█', filled));

        x += filled;

        canvas.DrawMapCharacterText(
            x,
            mapRow,
            ConsoleColor.DarkGray,
            new string('░', barWidth - filled));

        x += barWidth - filled;

        canvas.DrawMapCharacterText(
            x,
            mapRow,
            ConsoleColor.DarkGray,
            "] ");

        x += 2;

        canvas.DrawMapCharacterText(
            x,
            mapRow,
            textColor,
            label);

        int padding = labelWidth - label.Length;

        if (padding > 0)
        {
            canvas.DrawMapCharacterText(
                x + label.Length,
                mapRow,
                ConsoleColor.DarkGray,
                new string(' ', padding));
        }
    }

    // 冷却进度文字
    private static string FormatCooldown(double progress)
    {
        progress = Math.Clamp(progress, 0.0, 1.0);

        if (progress < 0.20)
            return "0%";

        if (progress < 0.40)
            return "20%";

        if (progress < 0.60)
            return "40%";

        if (progress < 0.80)
            return "60%";

        double percent = progress * 100.0;

        if (percent > 99.9)
            percent = 99.9;

        return $"{percent:0.0}%";
    }
}
