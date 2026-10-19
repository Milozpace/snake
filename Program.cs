using System;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;

while (true)
{
    Snake yuki = new();
    Game game = new();

    game.StartGame(yuki);

    if (game.State == Game.GameState.Dead)
    {
        if (game.GameOver() == Game.GameControl.Restart)
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

