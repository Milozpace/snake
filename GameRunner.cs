using System.Net;

class GameRunner
{
    private readonly GameConfig config;

    private Game game;
    private readonly Renderer renderer;

    private bool _isRunning;

    //Constructor
    public GameRunner(GameConfig config)
    {
        this.config = config;
        game = new(config);
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

        if (key == ConsoleKey.Spacebar)
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
            {
                game.ToggleDirection(game.snake1, direction);
            }
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
                renderer.RenderFrame(game, game.snake1);

                Thread.Sleep(1);
            }

            if (_isRunning)
            {
                if (HandleGameOverInput() == true)
                {
                    game = new(config);
                    continue;
                }
            }
        }
    }

}