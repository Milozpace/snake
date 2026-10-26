using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

string GameJson = File.ReadAllText("./Config/configClassic.json");
string SkinsJson = File.ReadAllText("./Skin/skins.json");
GameConfig config =
    JsonSerializer.Deserialize<GameConfig>(GameJson)
    ?? throw new InvalidDataException("Invalid game configuration");

JsonSerializerOptions skinJsonOptions = new();
skinJsonOptions.Converters.Add(new JsonStringEnumConverter());

SkinConfig skinConfig =JsonSerializer.Deserialize<SkinConfig>(
        SkinsJson,
        skinJsonOptions
    )
    ?? throw new InvalidDataException("Invalid skin configuration");


Console.Write("\x1b[?1049h");
Console.Clear();
Console.CursorVisible = false;

GameRunner gameRunner = new(config, skinConfig);
gameRunner.Rungame();

Console.CursorVisible = true;
Console.Clear();
Console.Write("\x1b[?1049l");

