using System;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;
//
while (true)
{
    Renderer renderer = new Renderer();
    Snake yuki = new Snake();
    Game game = new Game();

    game.StartGame(yuki, renderer);
    if (game.GameOver(renderer) == Game.GameControl.Exit)
        break;
}


Console.CursorVisible = true;
Console.Clear();
Console.Write("\x1b[?1049l");

