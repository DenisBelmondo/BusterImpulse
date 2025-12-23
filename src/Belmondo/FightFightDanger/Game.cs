namespace Belmondo.FightFightDanger;

using GameStateAutomaton = StateAutomaton<Game, Game.State>;
using PlaysimStateResult = StateAutomaton<Game, Game.State>.Result;

public class Game
{
    //
    // playsim states
    //

    public enum State
    {
        Exploring,
        Battling,
    }

    //
    // instance vars
    //

    private readonly GameContext _gameContext;

    public event Action? PlayerDamaged;
    public event Action? EnemyDamaged;
    public event Action? EnemyDied;

    public World? World;
    public Battle Battle;

    public readonly GameStateAutomaton StateAutomaton = new()
    {
        EnterFunction = EnterFunction,
        UpdateFunction = UpdateFunction,
    };

    public readonly TimerContext BattleWaitTimerContext;
    public readonly TimerContext ScreenTransitionTimerContext;
    public readonly Log Log = new(8);

    private static PlaysimStateResult EnterFunction(Game self, State currentState)
    {
        switch (currentState)
        {
            case State.Exploring:
                self.Log.Clear();
                self._gameContext.AudioService.ChangeMusic(MusicTrack.WanderingStage1);
                break;
            case State.Battling:
                self._gameContext.AudioService.ChangeMusic(MusicTrack.Battle);
                break;
        }

        return PlaysimStateResult.Continue;
    }

    private static PlaysimStateResult UpdateFunction(Game self, State currentState)
    {
        switch (currentState)
        {
            case State.Exploring:
            {
                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen) && self.BattleWaitTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    self.StartBattle();
                }

                if (self.BattleWaitTimerContext.CurrentStatus == TimerContext.Status.Running)
                {
                    return PlaysimStateResult.Continue;
                }

                if (self.World is not null)
                {
                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        var (X, Y) = Direction.ToInt32Tuple(self.World.Player.Transform.Direction);
                        var desiredX = self.World.Player.Transform.Position.X + X;
                        var desiredY = self.World.Player.Transform.Position.Y + Y;

                        GameLogic.TryToInteractWithChest(self._gameContext, ref self.World, (desiredX, desiredY));
                    }

                    GameLogic.UpdatePlayer(self._gameContext, ref self.World);
                    GameLogic.UpdateChests(self._gameContext, ref self.World);
                }

                break;
            }

            case State.Battling:
            {
                if (self.World is not null)
                {
                    self.World.Player.Value.RunningHealth += MathF.Sign(self.World.Player.Value.Current.Health - self.World.Player.Value.RunningHealth) * ((float)self._gameContext.TimeContext.Delta * 3);
                }

                if (self.Battle is not null)
                {
                    self.Battle.Update();

                    if (self.Battle.StateAutomaton.CurrentState is null)
                    {
                        return PlaysimStateResult.Goto(State.Exploring);
                    }

                    if (self.Battle.CurrentBattleGoon is not null)
                    {
                        foreach (var bullet in self.Battle.CurrentBattleGoon.Bullets)
                        {
                            if (bullet.Closeness >= 0.9 && bullet.Closeness < 0.95)
                            {
                                var shouldHurtPlayer = bullet.HorizontalDirection == -MathF.Sign(self.Battle.CurrentPlayingContext.PlayerDodgeT);

                                shouldHurtPlayer &= self.Battle.CurrentPlayingContext.PlayerInvulnerabilityT == 0;

                                if (shouldHurtPlayer)
                                {
                                    if (self.World is not null)
                                    {
                                        self.World.Player.Value.Current.Health -= 10;
                                    }

                                    self.Battle.CurrentPlayingContext.PlayerInvulnerabilityT = 1;
                                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                                }
                            }
                        }
                    }
                }

                break;
            }
        }

        return PlaysimStateResult.Continue;
    }

    public Game(GameContext gameContext)
    {
        _gameContext = gameContext;
        Battle = new(gameContext);
        BattleWaitTimerContext = new(gameContext.TimeContext);
        ScreenTransitionTimerContext = new(gameContext.TimeContext);

        BattleWaitTimerContext.TimedOut += () =>
        {
            StateAutomaton.ChangeState(State.Battling);
        };
    }

    public void StartBattle()
    {
        Battle?.Reset();
        BattleWaitTimerContext.Start(1);
        ScreenTransitionTimerContext.Start(2);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
    }

    public void Update()
    {
        StateAutomaton.Update(this);
        ScreenTransitionTimerContext.Update();
        BattleWaitTimerContext.Update();
    }
}
