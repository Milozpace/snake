using System;
using System.Reflection;
using System.Runtime.CompilerServices;

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;


GameConfig config = new();
GameRunner gameRunner = new(config);
gameRunner.Rungame();

Console.CursorVisible = true;
Console.Clear();
Console.Write("\x1b[?1049l");

