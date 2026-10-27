public class Player
{
    public int Id { get; }
    public int Score { get; private set; } = 0;
    public Snake snake { get; }
    public SnakeSkin snakeSkin { get; init; }
    public bool isAlive { get; private set; } = true;

    public Player(int id, Snake snake, SnakeSkin skin)
    {
        Id = id;
        this.snake = snake;
        snakeSkin = skin;
    }

    public void GetPoint(int point)
    {
        Score += point;
    }

    public void PlayerDie()
    {
        isAlive = false;
    }
}