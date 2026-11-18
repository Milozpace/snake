namespace Snake.Core;

public class Player(int id, Snake snake)
{
    //snake
    public Snake Snake { get; } = snake;
    //玩家数据
    public int Id { get; } = id;
    public int Score { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public Snake.SkillState SkillState => Snake.CurrentSkillState;
    public int SkillActiveRemainingMs => Snake.SkillActiveRemainingMs;
    public int SkillCooldownRemainingMs => Snake.SkillCooldownRemainingMs;

    //方法
    public void AddScore(int score) => Score += score;

    public void Die() => IsAlive = false;

}
