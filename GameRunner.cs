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
            //玩家1 按键映射
            Snake.Direction? newDirection1 = key switch
            {
                ConsoleKey.UpArrow => Snake.Direction.Up,
                ConsoleKey.DownArrow => Snake.Direction.Down,
                ConsoleKey.LeftArrow => Snake.Direction.Left,
                ConsoleKey.RightArrow => Snake.Direction.Right,
                _ => null
            };
            if (newDirection1 is { } direction1)
                game.ToggleDirection_1(direction1);

            if (key == ConsoleKey.M)
                game.ToggleBoost_1();


            //玩家2 按键映射
            Snake.Direction? newDirection2 = key switch
            {
                ConsoleKey.W => Snake.Direction.Up,
                ConsoleKey.S => Snake.Direction.Down,
                ConsoleKey.A => Snake.Direction.Left,
                ConsoleKey.D => Snake.Direction.Right,
                _ => null
            };

            if (newDirection2 is { } direction2)
                game.ToggleDirection_2(direction2);
            if (key == ConsoleKey.Q)
                game.ToggleBoost_2();
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
                    game = new(Config, SkinConfig);
                    continue;
                }
            }
        }
    }

}