public class Player : IPlayerView
{
    public int Id { get; }
    IReadOnlyList<Point> IPlayerView.Body => snake.Body;


    public int Score { get; private set; } = 0;
    public Snake snake { get; }
    public SnakeSkin SnakeSkin { get; init; }
    public bool isAlive { get; private set; } = true;

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
        isAlive = false;
    }
}
