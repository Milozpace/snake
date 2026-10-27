using System.Reflection.PortableExecutable;
using System.Text;

public class Renderer
{
    private readonly GameConfig config;
    private const int MapOriginX = 12;
    private const int MapOriginY = 7;

    private static readonly string[] AnsiForeground =
    [
        "\x1b[30m", // Black
        "\x1b[34m", // DarkBlue
        "\x1b[32m", // DarkGreen
        "\x1b[36m", // DarkCyan
        "\x1b[31m", // DarkRed
        "\x1b[35m", // DarkMagenta
        "\x1b[33m", // DarkYellow
        "\x1b[37m", // Gray
        "\x1b[90m", // DarkGray
        "\x1b[94m", // Blue
        "\x1b[92m", // Green
        "\x1b[96m", // Cyan
        "\x1b[91m", // Red
        "\x1b[95m", // Magenta
        "\x1b[93m", // Yellow
        "\x1b[97m", // White
    ];
    private static string ToAnsi(ConsoleColor color) =>
        AnsiForeground[(int)color];

    private FrameBuffer currentFrame;
    private FrameBuffer previousFrame;
    private bool isFirstFrame;

    public Renderer(GameConfig config)
    {
        this.config = config;
        currentFrame = new(
            MapOriginX + config.MapWidth * 2 + 10,
            MapOriginY + config.MapHeight + 5
        );
        previousFrame = new(
            MapOriginX + config.MapWidth * 2 + 10,
            MapOriginY + config.MapHeight + 5
        );

        isFirstFrame = true;
    }
    //工具方法
    private void DrawCanvaCell(int x, int y, ConsoleColor color)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        currentFrame.SetCell(screenX, screenY, new('█', color));
        currentFrame.SetCell(screenX + 1, screenY, new('█', color));
    }

    private void DrawCanvaText(int x, int y, ConsoleColor color, string content)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        for (int i = 0; i < content.Length; i++)
        {
            char text = content[i];
            currentFrame.SetCell(screenX + i, screenY, new(text, color));
        }
    }

    //明天再看
    private void DrawCanvaRichText(
    int x,
    int y,
    params (ConsoleColor Color, string Text)[] segments)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        foreach (var (color, text) in segments)
        {
            DrawScreenText(screenX, screenY, color, text);
            screenX += text.Length;
        }
    }

    private void DrawScreenText(int screenX, int screenY, ConsoleColor color, string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            currentFrame.SetCell(screenX + i, screenY, new(content[i], color));
        }
    }

    private void DrawMapRowText(int mapCharX, int mapRow, ConsoleColor color, string content)
    {
        DrawScreenText(MapOriginX + mapCharX, MapOriginY + mapRow, color, content);
    }

    private void DrawSkillMeter(
        double progress,
        string label,
        ConsoleColor playerColor,
        ConsoleColor fillColor,
        ConsoleColor textColor,
        int mapRow)
    {
        const int labelWidth = 9;

        int mapWidthChars = config.MapWidth * 2;
        int reservedWidth = 7 + 2 + labelWidth;
        int maxBarWidth = Math.Max(1, mapWidthChars - reservedWidth);
        int barWidth = Math.Clamp(config.MapWidth / 4, 6, 12);
        barWidth = Math.Min(barWidth, maxBarWidth);

        progress = Math.Clamp(progress, 0.0, 1.0);

        int filled = (int)Math.Floor(progress * barWidth);
        if (progress > 0 && filled == 0)
            filled = 1;
        filled = Math.Clamp(filled, 0, barWidth);

        int blockWidth = 7 + barWidth + 2 + labelWidth;
        int x = Math.Max(0, mapWidthChars - blockWidth);

        DrawMapRowText(x, mapRow, playerColor, "BOOST [");
        x += 7;

        DrawMapRowText(x, mapRow, fillColor, new string('█', filled));
        x += filled;

        DrawMapRowText(x, mapRow, ConsoleColor.DarkGray, new string('░', barWidth - filled));
        x += barWidth - filled;

        DrawMapRowText(x, mapRow, ConsoleColor.DarkGray, "] ");
        x += 2;

        DrawMapRowText(x, mapRow, textColor, label);

        int padding = labelWidth - label.Length;
        if (padding > 0)
        {
            DrawMapRowText(
                x + label.Length,
                mapRow,
                ConsoleColor.DarkGray,
                new string(' ', padding));
        }
    }

    private void DrawCenteredText(int y, ConsoleColor color, string content)
    {
        int screenCenterX = (config.MapWidth * 2 - content.Length) / 4; ;

        DrawCanvaText(screenCenterX, y, color, content);
    }


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


    //调用方法
    private void DrawMap()
    {
        for (int y = 0; y < config.MapHeight; y++)
        {
            for (int x = 0; x < config.MapWidth; x++)
            {
                if (x == 0 || y == 0 || x == config.MapWidth - 1 || y == config.MapHeight - 1)
                {
                    DrawCanvaCell(x, y, ConsoleColor.DarkGray);
                }
            }
        }
    }

    private void DrawPlayerInfo(Game game)
    {
        int play1RowHeight = game.PlayerList.Count == 1
            ? -1
            : -2;
        for (int i = 0; i < game.PlayerList.Count; i++)
        {
            Player player = game.PlayerList[i];

            ConsoleColor playerColor = player.Id == 1
                ? ConsoleColor.Green
                : ConsoleColor.Cyan;

            DrawCanvaRichText(
                1, play1RowHeight + i,
                (playerColor, $"P{player.Id}  "),
                (ConsoleColor.DarkGray, "SCORE "),
                (ConsoleColor.White, $"{player.Score:D3}")
            );
        }
    }

    private static int GetPlayerInfoStartRow(Game game) =>
        game.PlayerList.Count == 1 ? -1 : -2;

    private void DrawPlayerSkillbars(Game game)
    {
        int startRow = GetPlayerInfoStartRow(game);

        for (int i = 0; i < game.PlayerList.Count; i++)
        {
            DrawSkillbar(game.PlayerList[i], startRow + i);
        }
    }
    private void DrawHeader(Game game)
    {
        int titleHeight = game.gameMode == GameRunner.GameMode.SinglePlayer
            ? -2
            : -3;

        DrawCenteredText(titleHeight, ConsoleColor.DarkGreen, "◆  S N A K E  ◆");
        DrawPlayerInfo(game);
    }

    private void DrawFooter(GameRunner.GameMode gameMode)
    {
        DrawCanvaText(1, config.MapHeight, ConsoleColor.DarkGreen, "◦ ↑←↓→ + M  BOOST");
        DrawCanvaText(config.MapWidth - 5, config.MapHeight, ConsoleColor.DarkGray, "ESC QUIT");
        if (gameMode == GameRunner.GameMode.LocalVersus)
        {
            DrawCanvaText(1 + 10 ,config.MapHeight, ConsoleColor.DarkCyan, "◦ WASD + Q  BOOST");
        }
    }

    private void DrawSkillbar(Player player, int mapRow)
    {
        Snake snake = player.snake;
        ConsoleColor playerColor = player.Id == 1
            ? ConsoleColor.Green
            : ConsoleColor.Cyan;
        string key = player.Id == 1 ? "M" : "Q";

        if (snake.CurrentSkillState == Snake.SkillState.Ready)
        {
            DrawSkillMeter(
                progress: 1.0,
                label: $"READY [{key}]",
                playerColor: playerColor,
                fillColor: playerColor,
                textColor: playerColor,
                mapRow: mapRow);
            return;
        }

        if (snake.CurrentSkillState == Snake.SkillState.Active)
        {
            double progress =
                (double)snake._skillActiveTimerMs / snake.SkillActiveMs;

            double seconds = snake._skillActiveTimerMs / 1000.0;

            DrawSkillMeter(
                progress: progress,
                label: $"{seconds:0.0}s",
                playerColor: playerColor,
                fillColor: ConsoleColor.Yellow,
                textColor: ConsoleColor.Yellow,
                mapRow: mapRow);
            return;
        }

        if (snake.CurrentSkillState == Snake.SkillState.CoolingDown)
        {
            double progress =
                1.0 - (double)snake._skillCooldownTimerMs / snake.SkillCooldownMs;

            string label = FormatCooldown(progress);

            ConsoleColor color = progress >= 0.80
                ? playerColor
                : ConsoleColor.Gray;

            DrawSkillMeter(
                progress: progress,
                label: label,
                playerColor: playerColor,
                fillColor: color,
                textColor: color,
                mapRow: mapRow);
        }
    }

    private void DrawFood(IReadOnlyList<Point> foodList)
    {
        foreach (Point food in foodList)
        {
            DrawCanvaCell(food.X, food.Y, ConsoleColor.Yellow);
        }
    }

    private void DrawSnake(Player player)
    {
        DrawCanvaCell(player.snake.Head.X, player.snake.Head.Y, player.snakeSkin.HeadColor);
        foreach (Point segment in player.snake.Body.Skip(1))
        {
            DrawCanvaCell(segment.X, segment.Y, player.snakeSkin.BodyColor);
        }
    }

    private void DrawPause()
    {
        DrawCenteredText(config.MapHeight / 2, ConsoleColor.Blue, "P A U S E");
    }



    private void DrawGameOver(IReadOnlyList<Player> playerList)
    {
        int centerY = config.MapHeight / 2;
        DrawCenteredText(centerY - 3, ConsoleColor.Red, "G A M E  O V E R");
        DrawCenteredText(centerY, ConsoleColor.White, $"SCORE: {playerList[0].Score}");
        DrawCenteredText(centerY + 3, ConsoleColor.DarkGray, "'ENTER' TO RESTART");
        DrawCenteredText(centerY + 4, ConsoleColor.DarkGray, " or 'ESC' TO EXIT");
    }
    private void DrawCanvas(Game game)
    {
        DrawMap();
        DrawHeader(game);
        DrawPlayerSkillbars(game);
        DrawFooter(game.gameMode);
        DrawFood(game.FoodList);
        foreach (Player player in game.PlayerList)
        {
            DrawSnake(player);
        }
    }


    private void Present(FrameBuffer previousFrame, FrameBuffer currentFrame)
    {
        StringBuilder output = new();

        bool fullRedraw = isFirstFrame;

        if (fullRedraw)
            output.Append("\x1b[2J");

        for (int y = 0; y < currentFrame.Height; y++)
        {
            for (int x = 0; x < currentFrame.Width; x++)
            {
                Cell newCell = currentFrame.GetCell(x, y);
                Cell oldCell = previousFrame.GetCell(x, y);

                if (!fullRedraw && newCell == oldCell)
                    continue;

                output.Append($"\x1b[{y + 1};{x + 1}H");
                output.Append(ToAnsi(newCell.Color));
                output.Append(newCell.Character);
            }
        }

        output.Append("\x1b[0m");

        Console.Write(output.ToString());

        isFirstFrame = false;
    }

    //API
    public void RenderMenu()
    {
        currentFrame.Clear();//其实重启之后，画面也是衔接的，因为renderer还是原来那个，只是game重启了
        int centerY = config.MapHeight / 2;
        DrawMap();
        DrawCenteredText(centerY - 2, ConsoleColor.DarkGreen, "Choose   Your   GameMode");
        DrawCenteredText(centerY, ConsoleColor.White, "press  [1]: SinglePlayer");
        DrawCenteredText(centerY + 1, ConsoleColor.White, "press  [2]: LocalVersus");

        Present(previousFrame, currentFrame);
        (previousFrame, currentFrame) = (currentFrame, previousFrame);
    }
    public void RenderFrame(Game game)
    {
        currentFrame.Clear();
        DrawCanvas(game);

        if (game.State == Game.GameState.Paused)
            DrawPause();
        if (game.State == Game.GameState.GameOver)
            DrawGameOver(game.PlayerList);

        Present(previousFrame, currentFrame);
        (previousFrame, currentFrame) = (currentFrame, previousFrame);
    }
}