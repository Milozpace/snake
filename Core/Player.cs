namespace snake.Core;

using snake.Core.Primitives;

public class Player(int id, Snake snake) : IPlayerView
{
    //snake
    public Snake Snake { get; } = snake;
    //接口
    public int Id { get; } = id;
    public int Score { get; private set; }
    public bool IsAlive { get; private set; } = true;
    IReadOnlyList<Point> IPlayerView.Body => Snake.Body;
    public Snake.SkillState SkillState => Snake.CurrentSkillState;
    public int SkillActiveRemainingMs => Snake._skillActiveTimerMs;
    public int SkillCooldownRemainingMs => Snake._skillCooldownTimerMs;

    //方法
    public void AddScore(int score) => Score += score;

    public void Die() => IsAlive = false;

}
