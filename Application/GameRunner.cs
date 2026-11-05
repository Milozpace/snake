namespace snake.Application;

using System.Diagnostics;
using SnakeNetwork.Protocol;
using snake.Core;
using GameDirection = Core.Primitives.Direction;
using snake.Core.GameTypes;
using snake.Rendering;
using snake.Rendering.Skin;
using CoreGameState = Core.GameTypes.GameState;
using snake.Input;
using snake.Network;
using snake.Config;


public class GameRunner
{
    //Config and Member
    private readonly GameConfig _config;
    private readonly Renderer _renderer;

    //游戏状况
    private bool _isRunning;

    //计时器
    private readonly Stopwatch _renderClock = Stopwatch.StartNew();

    //net与发布模块
    private readonly MqttTransport? _network;
    private readonly WorldSnapshotBroadcaster? _snapshotBroadcaster;

    //Constructor
    public GameRunner(
        GameConfig config,
        SkinConfig skinConfig,
        MqttTransport? network = null)
    {
        _config = config;
        _network = network;
        _renderer = new(config, skinConfig);

        if (_network is not null)
            _snapshotBroadcaster = new(config, _network);

        _isRunning = true;
    }

    //键盘输入接口 （to game）
    private static GameMode? SelectGameMode()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
                return null;

            if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
                return GameMode.SinglePlayer;
            if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
                return GameMode.LocalVersus;
        }
    }

    private void HandlePlayingInput(Game game)
    {
        if (!Console.KeyAvailable)
            return;

        ConsoleKey key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
        {
            _isRunning = false;
            return;
        }
        else if (key == ConsoleKey.Spacebar)
        {
            game.TogglePause();
        }

        if (game.GameState != CoreGameState.Paused)
        {
            //判断第二套按键的绑定对象（根据创建游戏的gamemode）
            int secondBindingPlayerId;
            if (game.Mode == GameMode.SinglePlayer || _network != null)
                secondBindingPlayerId = 1;
            else
                secondBindingPlayerId = 2;

            //按键一
            GameDirection? newDirection1 = key.ToGameDirectionFromArrowKeys();
            if (newDirection1 is { } direction1)
                game.ToggleDirection(01, direction1);

            if (key == ConsoleKey.M)
                game.ToggleBoost(01);

            //按键二
            GameDirection? newDirection2 = key.ToGameDirectionFromWasdKeys();
            if (newDirection2 is { } direction2)
                game.ToggleDirection(secondBindingPlayerId, direction2);

            if (key == ConsoleKey.Q)
                game.ToggleBoost(secondBindingPlayerId);
        }
    }

    private void HandleRemoteInput(Game game) //1.接收input信息 2.解析 3.调用应用
    {
        if (_network is null)
            return;
        while (_network.TryReceive(out byte[]? received) && received is not null)
        {
            PlayerInput input = PlayerInput.Parser.ParseFrom(received);
            RemoteInputProcessor.ApplyRemoteInput(game, input);
        }
    }

    private static bool HandleGameOverInput()
    {
        while (true)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            if (key is ConsoleKey.Enter)
                return true;
            if (key is ConsoleKey.Escape)
                return false;
        }
    }


    //运行游戏
    public async Task RunGameAsync()
    {
        while (_isRunning)
        {
            //环节一：模式选择·创建游戏
            if (_network is null)
                _renderer.RenderMenu();

            GameMode? gameMode = _network is null
                ? SelectGameMode()
                : GameMode.OnlineVersus;

            if (gameMode is not GameMode selectedMode)
            {
                _isRunning = false;
                break;
            }

            var game = new Game(_config, selectedMode);

            //环节二：游戏循环（网络处理、游戏处理、渲染调用）
            while (game.GameState != CoreGameState.GameOver && _isRunning)
            {
                HandlePlayingInput(game);
                HandleRemoteInput(game);//网络调用1
                game.UpdateGameIfdue();

                //保证最后一帧能正常渲染与发布
                if (game.GameState == CoreGameState.GameOver)
                {
                    _renderer.RenderFrame(game);
                    if (_snapshotBroadcaster is not null)
                        await _snapshotBroadcaster.PublishFinalSnapshotAsync(game);
                    break;
                }
                _snapshotBroadcaster?.UpdateSnapshotBroadcast(game);

                if (_renderClock.ElapsedMilliseconds >= 40)
                {
                    _renderer.RenderFrame(game);
                    _renderClock.Restart();
                }

                Thread.Sleep(1);
            }

            //环节三：游戏结束交互
            if (_isRunning && HandleGameOverInput() == true)
                continue;
            else
                _isRunning = false;
        }
    }

}