using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

string json = File.ReadAllText("config.json");
GameConfig config = 
    JsonSerializer.Deserialize<GameConfig>(json) 
    ?? throw new InvalidDataException("Invalid game configuration");

Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;

GameRunner gameRunner = new(config);
gameRunner.Rungame();

Console.CursorVisible = true;
Console.Clear();
Console.Write("\x1b[?1049l");

