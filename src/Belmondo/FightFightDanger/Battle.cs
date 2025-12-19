namespace Belmondo.FightFightDanger;

public class Battle : IResettable
{
    public struct ChoosingContext
    {
        public int CurrentChoice;
    }

    public struct PlayingContext
    {
        public TimerContext CrosshairTimerContext;
        public double PlayerInvulnerabilityT;
        public float PlayerDodgeT;
        public float CrosshairT;
        public bool CrosshairIsVisible;

        public readonly bool CrosshairIsInRange(float minT, float maxT) => CrosshairT > minT && CrosshairT < maxT;
    }

    public static State<Battle> ChoosingState = State<Battle>.Empty;
    public static State<Battle> PlayingState = State<Battle>.Empty;
    public static State<Battle> VictoryState = State<Battle>.Empty;
    public static State<Battle> CrosshairWaitingState = State<Battle>.Empty;
    public static State<Battle> CrosshairAimingState = State<Battle>.Empty;
    public static State<Battle> CrosshairCountdownState = State<Battle>.Empty;
    public static State<Battle> CrosshairMissState = State<Battle>.Empty;
    public static State<Battle> CrosshairTargetState = State<Battle>.Empty;

    public static State<Battle> PlayerReadyState = State<Battle>.Empty;
    public static State<Battle> PlayerDodgeState = State<Battle>.Empty;

    public event Action? PlayerWon;
    public event Action? PlayerRan;
    public event Action? Penis;

    private readonly GameContext _gameContext;

    public readonly StateAutomaton<Battle> StateAutomaton = new();
    public readonly StateAutomaton<Battle> PlayerStateAutomaton = new();
    public readonly StateAutomaton<Battle> CrosshairStateAutomaton = new();
    public readonly TimerContext VictoryExitTimerContext;

    public ChoosingContext CurrentChoosingContext;
    public PlayingContext CurrentPlayingContext;
    public BattleGoon? CurrentBattleGoon;

    static Battle()
    {
        ChoosingState = new()
        {
            UpdateFunction = static self =>
            {
                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.BattleAttack))
                {
                    return State<Battle>.Goto(PlayingState);
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.BattleRun))
                {
                    return State<Battle>.Stop;
                }

                return State<Battle>.Continue;
            },
        };

        PlayingState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentBattleGoon?.StateAutomaton.ChangeState(BattleGoon.BeginAttackState);
                self.PlayerStateAutomaton.ChangeState(PlayerReadyState);
                self.CrosshairStateAutomaton.ChangeState(CrosshairCountdownState);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentBattleGoon is not null)
                {
                    self.CurrentBattleGoon.Update();

                    if (self.CurrentBattleGoon.StateAutomaton.CurrentState == BattleGoon.IdleState)
                    {
                        return State<Battle>.Goto(ChoosingState);
                    }

                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm) && self.CrosshairStateAutomaton.CurrentState == CrosshairAimingState)
                    {
                        if (self.CurrentPlayingContext.CrosshairIsInRange(-0.125f, 0.125f))
                        {
                            if (self.CurrentPlayingContext.CrosshairIsInRange(-0.01f, 0.01f))
                            {
                                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Crit);
                            }

                            self.CurrentBattleGoon.Bullets.Clear();
                            self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Clap);
                            self.CrosshairStateAutomaton.ChangeState(CrosshairTargetState);
                            self.CurrentBattleGoon.StateAutomaton.ChangeState(State<BattleGoon>.Empty);
                        }
                        else
                        {
                            self.CrosshairStateAutomaton.ChangeState(State<Battle>.Empty);
                            self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Miss);
                            self.CurrentPlayingContext.CrosshairIsVisible = false;
                        }
                    }

                    if (self.CurrentBattleGoon.StateAutomaton.PreviousState == BattleGoon.FlyOffscreenState && self.CurrentBattleGoon.CurrentAnimationContext.FlyOffscreenTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                    {
                        self._gameContext.AudioService.ChangeMusic(MusicTrack.Victory);
                        self.PlayerWon?.Invoke();
                        return State<Battle>.Goto(VictoryState);
                    }
                }

                self.PlayerStateAutomaton.Update(self);
                self.CrosshairStateAutomaton.Update(self);

                return State<Battle>.Continue;
            },

            ExitFunction = static self =>
            {
                self.CurrentPlayingContext.PlayerDodgeT = 0;
            },
        };

        VictoryState = new()
        {
            EnterFunction = static self =>
            {
                self.VictoryExitTimerContext.Reset();
            },

            UpdateFunction = static self =>
            {
                if (self.VictoryExitTimerContext.JustTimedOut)
                {
                    return State<Battle>.Stop;
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    self.Penis?.Invoke();
                    return State<Battle>.Stop;
                }

                return State<Battle>.Continue;
            },
        };

        PlayerReadyState = new()
        {
            UpdateFunction = static self =>
            {
                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.MoveLeft))
                {
                    self.CurrentPlayingContext.PlayerDodgeT -= 1f;
                    return State<Battle>.Goto(PlayerDodgeState);
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.MoveRight))
                {
                    self.CurrentPlayingContext.PlayerDodgeT += 1f;
                    return State<Battle>.Goto(PlayerDodgeState);
                }

                return State<Battle>.Continue;
            },
        };

        PlayerDodgeState = new()
        {
            UpdateFunction = static self =>
            {
                if (Math.Abs(self.CurrentPlayingContext.PlayerDodgeT) < 0.01)
                {
                    self.CurrentPlayingContext.PlayerDodgeT = 0;
                    return State<Battle>.Goto(PlayerReadyState);
                }

                if (self.CurrentPlayingContext.PlayerDodgeT < 0)
                {
                    self.CurrentPlayingContext.PlayerDodgeT += (float)self._gameContext.TimeContext.Delta * 4;

                    if (self.CurrentPlayingContext.PlayerDodgeT >= 0)
                    {
                        return State<Battle>.Goto(PlayerReadyState);
                    }
                }
                else if (self.CurrentPlayingContext.PlayerDodgeT > 0)
                {
                    self.CurrentPlayingContext.PlayerDodgeT -= (float)self._gameContext.TimeContext.Delta * 4;

                    if (self.CurrentPlayingContext.PlayerDodgeT <= 0)
                    {
                        return State<Battle>.Goto(PlayerReadyState);
                    }
                }

                return State<Battle>.Continue;
            },

            ExitFunction = static self =>
            {
                self.CurrentPlayingContext.PlayerDodgeT = 0;
            },
        };

        CrosshairCountdownState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairIsVisible = false;
                self.CurrentPlayingContext.CrosshairTimerContext.Start(3);
            },

            UpdateFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairTimerContext.Update();

                if (self.CurrentPlayingContext.CrosshairTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<Battle>.Goto(CrosshairAimingState);
                }

                return State<Battle>.Continue;
            },
        };

        CrosshairAimingState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairIsVisible = true;
            },

            UpdateFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairT = MathF.Sin((float)self._gameContext.TimeContext.Time * 2);
                return State<Battle>.Continue;
            },
        };

        CrosshairTargetState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairTimerContext.Start(0.5);
                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Clap);
            },

            UpdateFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairTimerContext.Update();

                if (self.CurrentPlayingContext.CrosshairTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    if (self.CurrentBattleGoon is not null)
                    {
                        var damage = 1;

                        if (self.CurrentPlayingContext.CrosshairIsInRange(-0.01f, 0.01f))
                        {
                            damage = 2;
                        }

                        self.CurrentBattleGoon.Damage(damage);
                        self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                    }

                    return State<Battle>.Stop;
                }

                return State<Battle>.Continue;
            },

            ExitFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairIsVisible = false;
            },
        };
    }

    public Battle(GameContext gameContext)
    {
        _gameContext = gameContext;
        VictoryExitTimerContext = new(gameContext.TimeContext);
        CurrentPlayingContext.CrosshairTimerContext = new(gameContext.TimeContext);
        Reset();
    }

    public void Reset()
    {
        StateAutomaton.ChangeState(ChoosingState);
        PlayerStateAutomaton.CurrentState = PlayerReadyState;
        CurrentBattleGoon = new BattleGoon(_gameContext);
    }

    public void Update()
    {
        CurrentPlayingContext.PlayerInvulnerabilityT -= _gameContext.TimeContext.Delta;

        if (CurrentPlayingContext.PlayerInvulnerabilityT < 0)
        {
            CurrentPlayingContext.PlayerInvulnerabilityT = 0;
        }

        StateAutomaton.Update(this);
        VictoryExitTimerContext.Update();
    }
}
