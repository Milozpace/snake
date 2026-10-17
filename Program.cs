using System;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;
//

Renderer renderer = new Renderer();
Snake yuki = new Snake();
Game game = new Game();

game.StartGame(yuki, renderer);

//
Console.ReadKey(true);
Console.CursorVisible = true;
Console.Write("\x1b[?1049l"); 

