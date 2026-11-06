
namespace snake.Rendering;

using snake.Config;
using snake.Core.GameTypes;
using snake.Views;

public class OverlayRenderer(GameConfig config)
{
    private readonly GameConfig _config = config;

    // 暂停界面
    public void DrawPause(CanvasPainter canvas)
    {
        canvas.DrawCenteredMapText(
            _config.MapHeight / 2,
            ConsoleColor.Blue,
            "P A U S E");
    }

    // 游戏结束界面
    public void DrawGameOver(
        CanvasPainter canvas,
        IWorldView game)
    {
        bool isVersus = game.Players.Count > 1;

        IPlayerView? winner = isVersus
            ? game.Players.FirstOrDefault(p => p.IsAlive)
            : null;

        string title = !isVersus
            ? "GAME OVER"
            : winner is null
                ? "DRAW"
                : $"PLAYER {winner.Id} WINS";

        ConsoleColor accent = !isVersus
            ? ConsoleColor.Red
            : winner is null
                ? ConsoleColor.Yellow
                : winner.Id == 1
                    ? ConsoleColor.Green
                    : ConsoleColor.Cyan;

        int mapWidthChars = _config.MapWidth * 2;

        int panelWidth = Math.Clamp(
            mapWidthChars - 8,
            30,
            52);

        int panelHeight = isVersus ? 8 : 7;

        int panelX = CanvasPainter.MapOriginX +
            (mapWidthChars - panelWidth) / 2;

        int panelY = CanvasPainter.MapOriginY +
            (_config.MapHeight - panelHeight) / 2;

        // 清空面板内部
        for (int row = 0; row < panelHeight; row++)
        {
            canvas.DrawScreenText(
                panelX,
                panelY + row,
                ConsoleColor.Gray,
                new string(' ', panelWidth));
        }

        // 上边框
        canvas.DrawScreenText(
            panelX,
            panelY,
            accent,
            "╔" + new string('═', panelWidth - 2) + "╗");

        // 左右边框
        for (int row = 1; row < panelHeight - 1; row++)
        {
            canvas.DrawScreenText(
                panelX,
                panelY + row,
                accent,
                "║");

            canvas.DrawScreenText(
                panelX + panelWidth - 1,
                panelY + row,
                accent,
                "║");
        }

        // 下边框
        canvas.DrawScreenText(
            panelX,
            panelY + panelHeight - 1,
            accent,
            "╚" + new string('═', panelWidth - 2) + "╝");

        // 标题
        canvas.DrawCenteredScreenText(
            panelX,
            panelY + 1,
            panelWidth,
            accent,
            title);

        // 分隔线
        canvas.DrawScreenText(
            panelX + 2,
            panelY + 2,
            ConsoleColor.DarkGray,
            new string('─', panelWidth - 4));

        if (isVersus)
        {
            // 双人结算
            for (int i = 0; i < game.Players.Count; i++)
            {
                IPlayerView player = game.Players[i];

                ConsoleColor playerColor = player.Id == 1
                    ? ConsoleColor.Green
                    : ConsoleColor.Cyan;

                canvas.DrawCenteredScreenRichText(
                    panelX,
                    panelY + 3 + i,
                    panelWidth,
                    (playerColor, $"P{player.Id}  "),
                    (ConsoleColor.DarkGray, "SCORE "),
                    (ConsoleColor.White, $"{player.Score:D3}"),
                    (ConsoleColor.DarkGray, "   LEN "),
                    (ConsoleColor.White, $"{player.Body.Count:D2}"));
            }

            canvas.DrawCenteredScreenRichText(
                panelX,
                panelY + 6,
                panelWidth,
                (ConsoleColor.Green, "ENTER"),
                (ConsoleColor.DarkGray, "  PLAY AGAIN"),
                (ConsoleColor.DarkGray, "   "),
                (ConsoleColor.Green, "ESC"),
                (ConsoleColor.DarkGray, "  QUIT"));
        }
        else
        {
            // 单人结算
            IPlayerView player = game.Players[0];

            canvas.DrawCenteredScreenRichText(
                panelX,
                panelY + 3,
                panelWidth,
                (ConsoleColor.DarkGray, "SCORE "),
                (ConsoleColor.White, $"{player.Score:D3}"),
                (ConsoleColor.DarkGray, "   LEN "),
                (ConsoleColor.White, $"{player.Body.Count:D2}"));

            canvas.DrawCenteredScreenRichText(
                panelX,
                panelY + 5,
                panelWidth,
                (ConsoleColor.Green, "ENTER"),
                (ConsoleColor.DarkGray, "  PLAY AGAIN"),
                (ConsoleColor.DarkGray, "   "),
                (ConsoleColor.Green, "ESC"),
                (ConsoleColor.DarkGray, "  QUIT"));
        }
    }

    // 主菜单
    public void DrawMenu(CanvasPainter canvas)
    {
        int mapWidthChars = _config.MapWidth * 2;

        int panelWidth = Math.Clamp(
            mapWidthChars - 16,
            32,
            50);

        const int panelHeight = 10;

        int x = CanvasPainter.MapOriginX +
            (mapWidthChars - panelWidth) / 2;

        int y = CanvasPainter.MapOriginY +
            (_config.MapHeight - panelHeight) / 2;

        // 上边框
        canvas.DrawScreenText(
            x,
            y,
            ConsoleColor.DarkGreen,
            "╔" + new string('═', panelWidth - 2) + "╗");

        // 下边框
        canvas.DrawScreenText(
            x,
            y + panelHeight - 1,
            ConsoleColor.DarkGreen,
            "╚" + new string('═', panelWidth - 2) + "╝");

        // 左右边框
        for (int row = 1; row < panelHeight - 1; row++)
        {
            canvas.DrawScreenText(
                x,
                y + row,
                ConsoleColor.DarkGreen,
                "║");

            canvas.DrawScreenText(
                x + panelWidth - 1,
                y + row,
                ConsoleColor.DarkGreen,
                "║");
        }

        // 标题
        canvas.DrawCenteredScreenText(
            x,
            y + 1,
            panelWidth,
            ConsoleColor.Green,
            "◆  S N A K E  ◆");

        // 分隔线
        canvas.DrawScreenText(
            x + 2,
            y + 2,
            ConsoleColor.DarkGray,
            new string('─', panelWidth - 4));

        // 模式选择
        canvas.DrawCenteredScreenText(
            x,
            y + 4,
            panelWidth,
            ConsoleColor.Green,
            "[1]  SINGLE PLAYER");

        canvas.DrawCenteredScreenText(
            x,
            y + 6,
            panelWidth,
            ConsoleColor.Cyan,
            "[2]  LOCAL VERSUS");

        // 操作提示
        canvas.DrawCenteredScreenText(
            x,
            y + panelHeight - 2,
            panelWidth,
            ConsoleColor.DarkGray,
            "1 / 2  START      ESC  QUIT");
    }
}
