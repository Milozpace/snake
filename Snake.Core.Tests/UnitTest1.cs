using GameSnake = snake.Core.Snake;
using snake.Config;
using snake.Core;
using snake.Core.Primitives;
using snake.Core.GameTypes;

namespace Snake.Core.Tests;

public class SnakeTests
{
    private static GameConfig CreateConfig() => new()
    {
        MapWidth = 40,
        MapHeight = 20,
        SimStepMs = 5,
        InitMoveIntervalMs = 180,
        MinMoveIntervalMs = 100,
        MoveIntervalDecreaseMs = 4,
        SkillActiveMs = 5000,
        SkillCooldownMs = 10000
    };

    [Fact]
    public void Snake_ShouldStartWithThreeSegments()
    {
        // Arrange：准备测试对象
        GameSnake snake = new(
            CreateConfig(),
            new Point(0, 0),
            Direction.Right
        );

        // Assert：验证结果
        Assert.Equal(3, snake.Body.Count);
        Assert.Equal(new Point(20, 10), snake.Head);
        Assert.Equal(Direction.Right, snake.CurrentDirection);
    }

    [Fact]
    public void Snake_ShouldRejectReverseDirection()
    {
        GameSnake snake = new(
            CreateConfig(),
            new Point(0, 0),
            Direction.Right
        );

        // 向左掉头是不允许的
        snake.RequestTurn(Direction.Left);

        // 推进 180ms，恰好移动一格
        for (int i = 0; i < 36; i++)
            snake.AdvanceStep();

        Assert.Equal(Direction.Right, snake.CurrentDirection);
        Assert.Equal(new Point(21, 10), snake.Head);
    }

    [Fact]
    public void Snake_BoostShouldEnterCooldownAndRecover()
    {
        GameSnake snake = new(
            CreateConfig(),
            new Point(0, 0),
            Direction.Right
        );

        // 初始技能可用
        Assert.Equal(GameSnake.SkillState.Ready, snake.CurrentSkillState);

        // 激活技能
        Assert.True(snake.TryBoost());
        Assert.Equal(GameSnake.SkillState.Active, snake.CurrentSkillState);

        // 激活期间不能重复使用
        Assert.False(snake.TryBoost());

        // 经过 5000ms，进入冷却
        for (int i = 0; i < 1000; i++)
            snake.AdvanceStep();

        Assert.Equal(GameSnake.SkillState.CoolingDown, snake.CurrentSkillState);

        // 再经过 10000ms，技能恢复
        for (int i = 0; i < 2000; i++)
            snake.AdvanceStep();

        Assert.Equal(GameSnake.SkillState.Ready, snake.CurrentSkillState);
    }

    [Fact]
    public void Game_ShouldCreateCorrectPlayerCount()
    {
        Game singleGame = new(CreateConfig(), GameMode.SinglePlayer);
        Game localGame = new(CreateConfig(), GameMode.LocalVersus);
        Game onlineGame = new(CreateConfig(), GameMode.OnlineVersus);

        Assert.Single(singleGame.PlayerList);
        Assert.Equal(2, localGame.PlayerList.Count);
        Assert.Equal(2, onlineGame.PlayerList.Count);
    }
}
