namespace Belmondo.FightFightDanger;

using BattleStateAutomaton = StateAutomaton<Battle, Battle.State>;
using CrosshairStateAutomaton = StateAutomaton<Battle, Battle.CrosshairState>;
using PlayerStateAutomaton = StateAutomaton<Battle, Battle.PlayerState>;

public class Battle : IResettable
{
    public enum State
    {
        Choosing,
        Playing,
        Victory,
    }

    public enum CrosshairState
    {
        Waiting,
        Aiming,
        CountingDown,
        Missing,
        Targeting,
    }

    public enum PlayerState
    {
        Ready,
        Dodging,
    }

    public struct PlayingContext
    {
        public Timer CrosshairTimer;
        public double PlayerInvulnerabilityT;
        public float PlayerDodgeT;
        public float CrosshairT;
        public bool CrosshairIsVisible;

        public readonly bool CrosshairIsInRange(float minT, float maxT) => CrosshairT > minT && CrosshairT < maxT;
    }

    public event Action? PlayerWon;
    public event Action? PlayerRan;
    public event Action? Finished;

    private readonly GameContext _gameContext;

    public readonly BattleStateAutomaton StateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case State.Playing:
                {
                    self.CurrentFoe?.StateAutomaton.ChangeState(FoeState.BeginAttacking);
                    self.PlayerStateAutomaton.ChangeState(PlayerState.Ready);
                    self.CrosshairStateAutomaton.ChangeState(CrosshairState.CountingDown);

                    break;
                }

                case State.Victory:
                {
                    self.VictoryExitTimer.Reset();
                    break;
                }
            }

            return BattleStateAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            var inputService = self._gameContext.InputService;

            switch (currentState)
            {
                case State.Choosing:
                {
                    if (inputService.ActionWasJustPressed(InputAction.BattleAttack))
                    {
                        return BattleStateAutomaton.Result.Goto(State.Playing);
                    }

                    if (inputService.ActionWasJustPressed(InputAction.BattleRun))
                    {
                        return BattleStateAutomaton.Result.Stop;
                    }

                    break;
                }

                case State.Playing:
                {
                    if (self.CurrentFoe is Foe foe)
                    {
                        if (foe.StateAutomaton.IsProcessingState(FoeState.Idle))
                        {
                            return BattleStateAutomaton.Result.Goto(State.Choosing);
                        }

                        bool shouldStartAttack = (
                            self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm)
                            && self.CrosshairStateAutomaton.IsProcessingState(CrosshairState.Aiming));

                        if (shouldStartAttack)
                        {
                            if (self.CurrentPlayingContext.CrosshairIsInRange(-0.125f, 0.125f))
                            {
                                if (self.CurrentPlayingContext.CrosshairIsInRange(-0.01f, 0.01f))
                                {
                                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Crit);
                                }

                                foe.Bullets.Clear();
                                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Clap);
                                self.CrosshairStateAutomaton.ChangeState(CrosshairState.Targeting);
                                foe.StateAutomaton.Stop();
                            }
                            else
                            {
                                self.CrosshairStateAutomaton.Stop();
                                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Miss);
                                self.CurrentPlayingContext.CrosshairIsVisible = false;
                            }
                        }
                    }

                    self.PlayerStateAutomaton.Update(self);
                    self.CrosshairStateAutomaton.Update(self);

                    break;
                }

                case State.Victory:
                {
                    if (self.VictoryExitTimer.JustTimedOut)
                    {
                        return BattleStateAutomaton.Result.Stop;
                    }

                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        self.Finished?.Invoke();
                        return BattleStateAutomaton.Result.Stop;
                    }

                    return BattleStateAutomaton.Result.Continue;
                }
            }

            return BattleStateAutomaton.Result.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case State.Playing:
                {
                    self.CurrentPlayingContext.PlayerDodgeT = 0;

                    break;
                }
            }

            return BattleStateAutomaton.Result.Continue;
        }
    };

    public readonly PlayerStateAutomaton PlayerStateAutomaton = new()
    {
        UpdateFunction = static (self, currentState) =>
        {
            var inputService = self._gameContext.InputService;

            switch (currentState)
            {
                case PlayerState.Ready:
                {
                    if (inputService.ActionWasJustPressed(InputAction.MoveLeft))
                    {
                        self.CurrentPlayingContext.PlayerDodgeT -= 1f;
                        return PlayerStateAutomaton.Result.Goto(PlayerState.Dodging);
                    }

                    if (inputService.ActionWasJustPressed(InputAction.MoveRight))
                    {
                        self.CurrentPlayingContext.PlayerDodgeT += 1f;
                        return PlayerStateAutomaton.Result.Goto(PlayerState.Dodging);
                    }

                    break;
                }

                case PlayerState.Dodging:
                {
                    if (Math.Abs(self.CurrentPlayingContext.PlayerDodgeT) < 0.01)
                    {
                        self.CurrentPlayingContext.PlayerDodgeT = 0;
                        return PlayerStateAutomaton.Result.Goto(PlayerState.Ready);
                    }

                    if (self.CurrentPlayingContext.PlayerDodgeT < 0)
                    {
                        self.CurrentPlayingContext.PlayerDodgeT += (float)self._gameContext.TimeContext.Delta * 4;

                        if (self.CurrentPlayingContext.PlayerDodgeT >= 0)
                        {
                            return PlayerStateAutomaton.Result.Goto(PlayerState.Ready);
                        }
                    }
                    else if (self.CurrentPlayingContext.PlayerDodgeT > 0)
                    {
                        self.CurrentPlayingContext.PlayerDodgeT -= (float)self._gameContext.TimeContext.Delta * 4;

                        if (self.CurrentPlayingContext.PlayerDodgeT <= 0)
                        {
                            return PlayerStateAutomaton.Result.Goto(PlayerState.Ready);
                        }
                    }

                    break;
                }
            }

            return PlayerStateAutomaton.Result.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case PlayerState.Dodging:
                {
                    self.CurrentPlayingContext.PlayerDodgeT = 0;
                    break;
                }
            }

            return PlayerStateAutomaton.Result.Continue;
        },
    };

    public readonly CrosshairStateAutomaton CrosshairStateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case CrosshairState.CountingDown:
                {
                    self.CurrentPlayingContext.CrosshairIsVisible = false;
                    self.CurrentPlayingContext.CrosshairTimer.Start(3);

                    break;
                }

                case CrosshairState.Aiming:
                {
                    self.CurrentPlayingContext.CrosshairIsVisible = true;
                    break;
                }

                case CrosshairState.Targeting:
                {
                    self.CurrentPlayingContext.CrosshairTimer.Start(0.5);
                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Clap);

                    break;
                }
            }

            return CrosshairStateAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case CrosshairState.CountingDown:
                {
                    self.CurrentPlayingContext.CrosshairTimer.Update();

                    if (self.CurrentPlayingContext.CrosshairTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return CrosshairStateAutomaton.Result.Goto(CrosshairState.Aiming);
                    }

                    break;
                }

                case CrosshairState.Aiming:
                {
                    self.CurrentPlayingContext.CrosshairT = MathF.Sin((float)self._gameContext.TimeContext.Time * 2);
                    break;
                }

                case CrosshairState.Targeting:
                {
                    self.CurrentPlayingContext.CrosshairTimer.Update();

                    if (self.CurrentPlayingContext.CrosshairTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        if (self.CurrentFoe is not null)
                        {
                            var damage = 1;

                            if (self.CurrentPlayingContext.CrosshairIsInRange(-0.01f, 0.01f))
                            {
                                damage = 2;
                            }

                            self.CurrentFoe.Damage(damage);
                            self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                        }

                        return CrosshairStateAutomaton.Result.Stop;
                    }

                    break;
                }
            }

            return CrosshairStateAutomaton.Result.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            if (currentState == CrosshairState.Targeting)
            {
                self.CurrentPlayingContext.CrosshairIsVisible = false;
            }

            return CrosshairStateAutomaton.Result.Continue;
        },
    };

    public readonly Timer VictoryExitTimer;

    public PlayingContext CurrentPlayingContext;
    public Foe? CurrentFoe;

    public Battle(GameContext gameContext)
    {
        _gameContext = gameContext;
        VictoryExitTimer = new(gameContext.TimeContext);
        CurrentPlayingContext.CrosshairTimer = new(gameContext.TimeContext);
        Reset();
    }

    public void SetFoe(Foe foe)
    {
        if (CurrentFoe is not null)
        {
            CurrentFoe.Defeated -= OnFoeDefeated;
        }

        CurrentFoe = foe;
        CurrentFoe.Defeated += OnFoeDefeated;

        foe.RenderThing.ShapeType = foe.Type switch
        {
            FoeType.Turret => ShapeType.Turret,
            FoeType.Goon => ShapeType.Goon,
            _ => ShapeType.Placeholder,
        };
    }

    public void Reset()
    {
        StateAutomaton.ChangeState(State.Choosing);
        PlayerStateAutomaton.ChangeState(PlayerState.Ready);
        SetFoe(new Foe(FoeType.Turret, _gameContext));
    }

    public void Update()
    {
        CurrentPlayingContext.PlayerInvulnerabilityT -= _gameContext.TimeContext.Delta;

        if (CurrentPlayingContext.PlayerInvulnerabilityT < 0)
        {
            CurrentPlayingContext.PlayerInvulnerabilityT = 0;
        }

        CurrentFoe?.Update();
        StateAutomaton.Update(this);
        VictoryExitTimer.Update();
    }

    public void OnFoeDefeated()
    {
        _gameContext.AudioService.ChangeMusic(MusicTrack.Victory);
        PlayerWon?.Invoke();
        StateAutomaton.ChangeState(State.Victory);
    }
}
