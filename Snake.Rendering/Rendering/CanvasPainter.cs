
namespace snake.Rendering;

using snake.Config;

public class CanvasPainter(FrameBuffer frame, GameConfig config)
{
    public const int MapOriginX = 12;
    public const int MapOriginY = 7;

    private readonly FrameBuffer _frame = frame;
    private readonly GameConfig _config = config;


    // 地图格子：一个格子占用两个屏幕字符
    public void DrawMapCell(int x, int y, ConsoleColor color)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        _frame.SetCell(screenX, screenY, new('█', color));
        _frame.SetCell(screenX + 1, screenY, new('█', color));
    }

    // 地图坐标文字
    public void DrawMapText(int x, int y, ConsoleColor color, string content)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        DrawScreenText(screenX, screenY, color, content);
    }

    // 地图坐标富文本
    public void DrawMapRichText(
        int x,
        int y,
        params (ConsoleColor Color, string Text)[] segments)
    {
        int screenX = MapOriginX + x * 2;
        int screenY = MapOriginY + y;

        DrawScreenRichText(screenX, screenY, segments);
    }

    // 屏幕字符坐标文字
    public void DrawScreenText(
        int x,
        int y,
        ConsoleColor color,
        string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            _frame.SetCell(
                x + i,
                y,
                new(content[i], color));
        }
    }

    // 屏幕字符坐标富文本
    public void DrawScreenRichText(
        int x,
        int y,
        params (ConsoleColor Color, string Text)[] segments)
    {
        foreach (var (color, text) in segments)
        {
            DrawScreenText(x, y, color, text);
            x += text.Length;
        }
    }

    // 地图字符坐标文字（用于技能条）
    public void DrawMapCharacterText(
        int x,
        int y,
        ConsoleColor color,
        string content)
    {
        DrawScreenText(
            MapOriginX + x,
            MapOriginY + y,
            color,
            content);
    }

    // 地图区域居中文字
    public void DrawCenteredMapText(
        int y,
        ConsoleColor color,
        string content)
    {
        int x = (_config.MapWidth * 2 - content.Length) / 4;

        DrawMapText(x, y, color, content);
    }

    // 面板内居中文字
    public void DrawCenteredScreenText(
        int panelX,
        int screenY,
        int panelWidth,
        ConsoleColor color,
        string content)
    {
        int x = panelX + (panelWidth - content.Length) / 2;

        DrawScreenText(x, screenY, color, content);
    }

    // 面板内居中富文本
    public void DrawCenteredScreenRichText(
        int panelX,
        int screenY,
        int panelWidth,
        params (ConsoleColor Color, string Text)[] segments)
    {
        int totalWidth = 0;

        foreach (var segment in segments)
            totalWidth += segment.Text.Length;

        int x = panelX + (panelWidth - totalWidth) / 2;

        DrawScreenRichText(x, screenY, segments);
    }
}
