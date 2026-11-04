namespace snake.Core;

using snake.Core.Primitives;

public class Player : IPlayerView
{
    public int Id { get; }
    IReadOnlyList<Point> IPlayerView.Body => Snake.Body;

    //如你所见，这里的一大堆东西都是为了接口而丢出来的东西，但是这些不应该是snake 的东西吗？所以怎么办？
    public int Score { get; private set; } = 0;
    public Snake.SkillState SkillState => Snake.CurrentSkillState;
    public int SkillActiveRemainingMs => Snake._skillActiveTimerMs;
    public int SkillCooldownRemainingMs => Snake._skillCooldownTimerMs;
    public Snake Snake { get; } //这条蛇就是在这里吧！ 但是关系是？
  
    public bool IsAlive { get; private set; } = true;

    public Player(int id, Snake snake) //这里优化语法（什么use primary constructor？）
    {
        Id = id;
        Snake = snake;
    }

    public void AddScore(int score) //这下 命名应该没事了……
    {
        Score += score;
    }

    public void PlayerDie()//似乎一条的可以写的更简洁？
    {
        IsAlive = false;
    }
}
