using SnakeNetwork.Protocol;

public class GuestPlayerView : IPlayerView
{
    private readonly PlayerSnapshot _snapshot;

    public int Id => _snapshot.PlayerId;
    public int Score => _snapshot.Score;
    public bool IsAlive => _snapshot.IsAlive;
    public IReadOnlyList<Point> Body { get; }
    public Snake.SkillState SkillState { get; }
    public int SkillCooldownRemainingMs => _snapshot.CooldownRemainingMs;
    public int SkillActiveRemainingMs => _snapshot.ActiveRemainingMs;


    public GuestPlayerView(PlayerSnapshot snapshot)
    {
        _snapshot = snapshot;


        Body = snapshot.Body
            .Select(p => new Point(p.X, p.Y))
            .ToArray();

        SkillState = _snapshot.SkillStatus switch
        {
            SkillStatus.Ready => Snake.SkillState.Ready,
            SkillStatus.Active => Snake.SkillState.Active,
            SkillStatus.CoolingDown => Snake.SkillState.CoolingDown,
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot.SkillStatus))
        };
    }
}