using System;
using System.Text.Json;
using Godot;
using Snake.Config;

namespace Snake.Godot;

public static class GodotConfigLoader
{
	public static GameConfig Load(string path)
	{
		using var file = FileAccess.Open(
			path,
			FileAccess.ModeFlags.Read
		);

		if (file is null)
		{
			throw new InvalidOperationException(
				$"无法读取配置：{path}，错误：{FileAccess.GetOpenError()}"
			);
		}

		string json = file.GetAsText();

		GameConfig config =
			JsonSerializer.Deserialize<GameConfig>(json)
			?? throw new InvalidOperationException("配置内容无效");

		if (config.MapWidth <= 0 ||
			config.MapHeight <= 0 ||
			config.SimStepMs <= 0)
		{
			throw new InvalidOperationException(
                "地图尺寸和模拟步长必须大于零"
			);
		}

		return config;
	}
}
