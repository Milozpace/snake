
namespace Snake.Rendering;

using Snake.Config;
using Snake.Core.GameTypes;
using Snake.Rendering.Skin;
using Snake.Views;

public class Renderer
{
    private readonly GameConfig _config;

    // 绘制模块
    private readonly BoardRenderer _boardRenderer;
    private readonly HudRenderer _hudRenderer;
    private readonly OverlayRenderer _overlayRenderer;

    // 终端输出
    private readonly ConsolePresenter _presenter;

    // 双缓冲
    private FrameBuffer _currentFrame;
    private FrameBuffer _previousFrame;
    private bool _isFirstFrame = true;

    public Renderer(
        GameConfig config,
        SkinConfig skinConfig)
    {
        _config = config;

        _boardRenderer = new(config, skinConfig);
        _hudRenderer = new(config);
        _overlayRenderer = new(config);

        _presenter = new();

        int width = CanvasPainter.MapOriginX +
            config.MapWidth * 2 + 10;

        int height = CanvasPainter.MapOriginY +
            config.MapHeight + 5;

        _currentFrame = new(width, height);
        _previousFrame = new(width, height);
    }

    // 准备新的一帧
    private CanvasPainter BeginFrame()
    {
        _currentFrame.Clear();

        return new CanvasPainter(
            _currentFrame,
            _config);
    }

    // 输出并交换缓冲区
    private void PresentFrame()
    {
        _presenter.Present(
            _previousFrame,
            _currentFrame,
            _isFirstFrame);

        _isFirstFrame = false;

        (_previousFrame, _currentFrame) =
            (_currentFrame, _previousFrame);
    }

    // 主菜单
    public void RenderMenu()
    {
        CanvasPainter canvas = BeginFrame();

        _boardRenderer.DrawMap(canvas);

        _overlayRenderer.DrawMenu(canvas);

        PresentFrame();
    }

    // 游戏画面
    public void RenderFrame(WorldFrame frame)
    {
        CanvasPainter canvas = BeginFrame();

        // 游戏地图
        _boardRenderer.DrawMap(canvas);

        // 游戏信息
        _hudRenderer.DrawHeader(canvas, frame);
        _hudRenderer.DrawPlayerSkillbars(canvas, frame);
        _hudRenderer.DrawFooter(canvas, frame);

        // 游戏实体
        _boardRenderer.DrawFood(canvas, frame.Foods);
        _boardRenderer.DrawSnakes(canvas, frame.Players);

        // 覆盖界面
        if (frame.Status == GameState.Paused)
            _overlayRenderer.DrawPause(canvas);

        if (frame.Status == GameState.GameOver)
            _overlayRenderer.DrawGameOver(canvas, frame);

        PresentFrame();
    }
}
