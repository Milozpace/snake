public class GameConfig
{
    //Map
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }

    //Time
    public int SimStepMs { get; init; }
    public int InitMoveIntervalMs { get; init; }
    public int SkillCooldownMs { get; init; }
    public int SkillActiveMs { get; init; }

    public int MinMoveIntervalMs { get; init; }
    public int MoveIntervalDecreaseMs { get; init; }

    public int FoodSpawnIntervalMs { get; init; }
    public int FoodScore { get; init; }

}

