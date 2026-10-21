using System;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;

while (true)
{
    GameConfig config = new();
    Snake yuki = new();
    Game game = new(config);
    Renderer renderer = new(config);

    game.StartGame(yuki, renderer);

    if (game.State == Game.GameState.Dead)
    {
        if (game.GameOver(renderer) == Game.GameControl.Restart)
        {
            continue;
        }
        else
        {
            break;
        }
    }
    else
    {
        break;
    }
}

Console.CursorVisible = true;
Console.Clear();
Console.Write("\x1b[?1049l");

