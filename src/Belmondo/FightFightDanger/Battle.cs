namespace Belmondo.FightFightDanger;

public class Battle : IResettable
{
    public struct ChoosingContext
    {
        public int CurrentChoice;
    }

    public struct PlayingContext
    {
        public float PlayerDodgeT;
        public double PlayerInvulnerabilityT;
        public float CrosshairT;
        public double CrosshairCountdownSecondsLeft;

        public readonly bool CrosshairIsInRange(float minT, float maxT) => CrosshairT > minT && CrosshairT < maxT;
    }

    public static event Action<Battle>? PlayerRan;

    public static State<Battle> ChoosingState = State<Battle>.Empty;
    public static State<Battle> PlayingState = State<Battle>.Empty;
    public static State<Battle> WaitState = State<Battle>.Empty;
    public static State<Battle> CrosshairWaitingState = State<Battle>.Empty;
    public static State<Battle> CrosshairAimingState = State<Battle>.Empty;
    public static State<Battle> CrosshairCountdownState = State<Battle>.Empty;

    public static State<Battle> PlayerReadyState = State<Battle>.Empty;
    public static State<Battle> PlayerDodgeState = State<Battle>.Empty;

    private readonly GameContext _gameContext;

    public readonly StateAutomaton<Battle> StateAutomaton = new();
    public readonly StateAutomaton<Battle> PlayerStateAutomaton = new();
    public readonly StateAutomaton<Battle> CrosshairStateAutomaton = new();

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

                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm) && self.CrosshairStateAutomaton.CurrentState == Battle.CrosshairAimingState && self.CurrentPlayingContext.CrosshairIsInRange(-0.125f, 0.125f))
                    {
                        self.CurrentBattleGoon.Damage();
                        self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                        self.CrosshairStateAutomaton.ChangeState(CrosshairWaitingState);

                        if (self.CurrentBattleGoon.StateAutomaton.CurrentState == BattleGoon.IdleState)
                        {
                            return State<Battle>.Goto(ChoosingState);
                        }
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
        };

        CrosshairCountdownState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairCountdownSecondsLeft = 3;
            },

            UpdateFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairCountdownSecondsLeft -= self._gameContext.TimeContext.Delta;

                if (self.CurrentPlayingContext.CrosshairCountdownSecondsLeft <= 0)
                {
                    return State<Battle>.Goto(CrosshairAimingState);
                }

                return State<Battle>.Continue;
            },
        };

        CrosshairAimingState = new()
        {
            UpdateFunction = static self =>
            {
                self.CurrentPlayingContext.CrosshairT = MathF.Sin((float)self._gameContext.TimeContext.Time * 2);
                return State<Battle>.Continue;
            },
        };
    }

    public Battle(GameContext gameContext)
    {
        _gameContext = gameContext;
        Reset();
    }

    public void Reset()
    {
        StateAutomaton.CurrentState = ChoosingState;
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
    }
}
