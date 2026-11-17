
namespace snake.Rendering;

using System.Collections.Immutable;
using snake.Config;
using snake.Core.Primitives;
using snake.Rendering.Skin;
using snake.Views;

public class BoardRenderer(GameConfig config, SkinConfig skinConfig)
{
    private readonly GameConfig _config = config;
    private readonly SkinConfig _skinConfig= skinConfig;


    public void DrawMap(CanvasPainter canvas)
    {
        for (int y = 0; y < _config.MapHeight; y++)
        {
            for (int x = 0; x < _config.MapWidth; x++)
            {
                if (x == 0 ||
                    y == 0 ||
                    x == _config.MapWidth - 1 ||
                    y == _config.MapHeight - 1)
                {
                    canvas.DrawMapCell(
                        x,
                        y,
                        ConsoleColor.DarkGray);
                }
            }
        }
    }

    public void DrawFood(
        CanvasPainter canvas,
        ImmutableArray<Point> foods)
    {
        foreach (Point food in foods)
        {
            canvas.DrawMapCell(
                food.X,
                food.Y,
                ConsoleColor.Yellow);
        }
    }

    public void DrawSnakes(
        CanvasPainter canvas,
        ImmutableArray<PlayerFrame> players)
    {
        foreach (PlayerFrame player in players)
        {
            DrawSnake(canvas, player);
        }
    }

    private void DrawSnake(
        CanvasPainter canvas,
        PlayerFrame player)
    {
        if (player.Body.IsEmpty)
            return;

        SnakeSkin skin = player.Id switch
        {
            1 => _skinConfig.Player1,
            2 => _skinConfig.Player2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(player.Id))
        };

        Point head = player.Body[0];

        canvas.DrawMapCell(
            head.X,
            head.Y,
            skin.HeadColor);

        foreach (Point segment in player.Body.Skip(1))
        {
            canvas.DrawMapCell(
                segment.X,
                segment.Y,
                skin.BodyColor);
        }
    }
}
