
public class SkinConfig
{
    public required SnakeSkin Player1 { get; init; }
    public required SnakeSkin Player2 { get; init; }
}



public record SnakeSkin(
    ConsoleColor HeadColor,
    ConsoleColor BodyColor
);