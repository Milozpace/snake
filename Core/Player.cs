namespace snake.Core;

using snake.Rendering.Skin;

public class Player : IPlayerView
{
    public int Id { get; }
    IReadOnlyList<Point> IPlayerView.Body => snake.Body;


    public int Score { get; private set; } = 0;
    public Snake.SkillState SkillState => snake.CurrentSkillState;
    public int SkillActiveRemainingMs => snake._skillActiveTimerMs;
    public int SkillCooldownRemainingMs => snake._skillCooldownTimerMs;
    public Snake snake { get; }
    public SnakeSkin SnakeSkin { get; init; }
    public bool IsAlive { get; private set; } = true;

    public Player(int id, Snake snake, SnakeSkin skin)
    {
        Id = id;
        this.snake = snake;
        SnakeSkin = skin;
    }

    public void GetPoint(int point)
    {
        Score += point;
    }

    public void PlayerDie()
    {
        IsAlive = false;
    }
}
