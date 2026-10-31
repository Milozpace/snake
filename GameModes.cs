/// <summary>
/// Mode metadata shared by the start menu and game setup. Online play never
/// enters the start menu: its room/lobby flow already knows it is a versus
/// match, so the mode is selected by the network entry point instead.
/// </summary>
public static class GameModes
{
    /// <summary>Modes that can be selected from the local start menu.</summary>
    public static IReadOnlyList<GameRunner.GameMode> StartMenu { get; } =
    [
        GameRunner.GameMode.SinglePlayer,
        GameRunner.GameMode.LocalVersus,
    ];

    /// <summary>Online play skips the start menu and starts directly as a duel.</summary>
    public static bool UsesStartMenu(this GameRunner.GameMode mode) =>
        mode is GameRunner.GameMode.SinglePlayer or GameRunner.GameMode.LocalVersus;

    public static bool IsOnline(this GameRunner.GameMode mode) =>
        mode is GameRunner.GameMode.OnlineVersus;

    public static int PlayerCount(this GameRunner.GameMode mode) =>
        mode is GameRunner.GameMode.SinglePlayer ? 1 : 2;

    public static string DisplayName(this GameRunner.GameMode mode) =>
        mode switch
        {
            GameRunner.GameMode.SinglePlayer => "SINGLE PLAYER",
            GameRunner.GameMode.LocalVersus => "LOCAL VERSUS",
            GameRunner.GameMode.OnlineVersus => "ONLINE VERSUS",
            _ => mode.ToString().ToUpperInvariant(),
        };
}
