using System.Text.Json;
using System.Text.Json.Serialization;
using snake.Config;
using snake.Rendering.Skin;

namespace snake.Application.Startup;

internal static class AppConfigLoader
{
    public static (GameConfig GameConfig, SkinConfig SkinConfig) Load()
    {
        string baseDir = AppContext.BaseDirectory;

        string gamePath = Path.Combine(
            baseDir, "Config", "configClassic.json");

        string skinPath = Path.Combine(
            baseDir, "Rendering", "Skin", "skins.json");

        string gameJson = File.ReadAllText(gamePath);
        string skinsJson = File.ReadAllText(skinPath);

        GameConfig gameConfig =
            JsonSerializer.Deserialize<GameConfig>(gameJson)
            ?? throw new InvalidDataException(
                "Invalid game configuration");

        JsonSerializerOptions skinJsonOptions = new();
        skinJsonOptions.Converters.Add(new JsonStringEnumConverter());

        SkinConfig skinConfig =
            JsonSerializer.Deserialize<SkinConfig>(
                skinsJson,
                skinJsonOptions
            )
            ?? throw new InvalidDataException(
                "Invalid skin configuration");

        return (gameConfig, skinConfig);
    }
}