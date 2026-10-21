public class GameConfig
{
    //但是offset是渲染器的工作啦！
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }

    public int InitMoveIntervalMs { get; init; }
    public int FoodSpawnIntervalMs { get; init; }

    public GameConfig()
    {
        MapWidth = 40;
        MapHeight = 20;

        InitMoveIntervalMs = 200;
        FoodSpawnIntervalMs = 5000;
    }
}

