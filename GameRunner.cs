using System.Diagnostics;
using System.Net;

class GameRunner
{
    private readonly GameConfig Config;
    private readonly SkinConfig SkinConfig;
    private Game game;
    private readonly Renderer renderer;

    private bool _isRunning;

    Stopwatch _renderClock = Stopwatch.StartNew();

    //Constructor
    public GameRunner(GameConfig config, SkinConfig skinConfig)
    {
        Config = config;
        SkinConfig = skinConfig;
        game = new(config, skinConfig);
        renderer = new(config);

        _isRunning = true;
    }


    //键盘输入接口 （to game）
    private void HandlePlayingInput()
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            _isRunning = false;
            return;
        }
        else if (key == ConsoleKey.Spacebar)
        {
            game.TogglePause();
        }

        if (game.State != Game.GameState.Paused)
        {
            Snake.Direction? newdirection = key switch
            {
                ConsoleKey.UpArrow => Snake.Direction.Up,
                ConsoleKey.DownArrow => Snake.Direction.Down,
                ConsoleKey.LeftArrow => Snake.Direction.Left,
                ConsoleKey.RightArrow => Snake.Direction.Right,
                _ => null
            };
            if (newdirection is { } direction)
                game.ToggleDirection_1(direction);

            if (key == ConsoleKey.M)
                game.ToggleBoost_1();
        }
    }

    private bool HandleGameOverInput()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key is ConsoleKey.Enter or ConsoleKey.Escape)
            {
                switch (key)
                {
                    case ConsoleKey.Enter:
                        return true;
                    case ConsoleKey.Escape:
                        {
                            _isRunning = false;
                            return false;
                        }
                }
            }
        }

    }

    //运行游戏
    public void Rungame()
    {
        while (_isRunning)
        {
            while (game.State != Game.GameState.GameOver && _isRunning)
            {
                HandlePlayingInput();
                game.UpdateGameIfdue();

                if (game.State == Game.GameState.GameOver)
                {
                    renderer.RenderFrame(game);
                    continue;
                }

                if (_renderClock.ElapsedMilliseconds >= 40)
                {
                    renderer.RenderFrame(game);
                    _renderClock.Restart();
                }


                Thread.Sleep(1);
            }

            if (_isRunning)
            {
                if (HandleGameOverInput() == true)
                {
                    game = new(Config,SkinConfig);
                    continue;
                }
            }
        }
    }

}