using System.Numerics;

namespace Belmondo.FightFightDanger;

using PopUpStateAutomaton = StateAutomaton<UIState, UIState.PopUpState>;
using BattleVictoryScreenAutomaton = StateAutomaton<UIState, UIState.BattleVictoryScreenState>;
using MugshotStateAutomaton = StateAutomaton<UIState.MugshotStateContext, UIState.MugshotState>;

public class UIState(GameContext gameContext) : IThinker
{
    public enum PopUpState
    {
        Appearing,
        StickingAround,
        Disappearing,
    }

    public enum BattleVictoryScreenState
    {
        Appearing,
        StickingAround,
        Disappearing,
    }

    public enum MugshotState
    {
        Shaking,
    }

    public class MugshotStateContext(TimeContext timeContext) : IThinker
    {
        private readonly TimeContext _timeContext = timeContext;
        public readonly Timer ShakeTimer = new(timeContext);

        public readonly MugshotStateAutomaton StateAutomaton = new()
        {
            EnterFunction = static (self, currentState) =>
            {
                self.ShakeTimer.Start(0.25);
                return MugshotStateAutomaton.Result.Continue;
            },

            UpdateFunction = static (self, currentState) =>
            {
                if (currentState == MugshotState.Shaking)
                {
                    self.ShakeOffset =
                        Vector2.One
                        * (float)Math2.SampleTriangleWave(self._timeContext.Time * 50.0)
                        * 2;
                }

                if (self.ShakeTimer.CurrentStatus == Timer.Status.Stopped)
                {
                    return MugshotStateAutomaton.Result.Stop;
                }

                return MugshotStateAutomaton.Result.Continue;
            },

            ExitFunction = static (self, currentState) =>
            {
                if (currentState == MugshotState.Shaking)
                {
                    self.ShakeOffset = Vector2.Zero;
                }

                return MugshotStateAutomaton.Result.Continue;
            },
        };

        public Vector2 ShakeOffset;

        public void Update()
        {
            StateAutomaton.Update(this);
            ShakeTimer.Update();
        }
    }

    private readonly GameContext _gameContext = gameContext;

    public readonly PopUpStateAutomaton PopUpStateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case PopUpState.Appearing:
                {
                    self.PopUpT = 0;
                    break;
                }

                case PopUpState.StickingAround:
                {
                    self.PopUpWaitT = 0;
                    break;
                }

                case PopUpState.Disappearing:
                {
                    break;
                }
            }

            return PopUpStateAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case PopUpState.Appearing:
                {
                    self.PopUpT += (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                    if (self.PopUpT >= 1)
                    {
                        return PopUpStateAutomaton.Result.Goto(PopUpState.StickingAround);
                    }

                    break;
                }

                case PopUpState.StickingAround:
                {
                    self.PopUpWaitT += (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault();

                    if (self.PopUpWaitT >= 3)
                    {
                        return PopUpStateAutomaton.Result.Goto(PopUpState.Disappearing);
                    }

                    break;
                }

                case PopUpState.Disappearing:
                {
                    self.PopUpT -= (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                    if (self.PopUpT <= 0)
                    {
                        return PopUpStateAutomaton.Result.Stop;
                    }

                    break;
                }
            }

            return PopUpStateAutomaton.Result.Continue;
        },
    };

    public readonly BattleVictoryScreenAutomaton BattleVictoryStateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case BattleVictoryScreenState.Appearing:
                {
                    if (currentState == BattleVictoryScreenState.Appearing)
                    {
                        self.BattleVictoryWipeT = 0;
                    }

                    break;
                }

                case BattleVictoryScreenState.Disappearing:
                {
                    self.BattleVictoryWipeT = 0.5;
                    break;
                }
            }

            return BattleVictoryScreenAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case BattleVictoryScreenState.Appearing:
                {
                    self.BattleVictoryWipeT += self._gameContext.TimeContext.Delta / 2;

                    if (self.BattleVictoryWipeT >= 0.5)
                    {
                        self.BattleVictoryWipeT = 0.5;
                        return BattleVictoryScreenAutomaton.Result.Stop;
                    }

                    break;
                }

                case BattleVictoryScreenState.Disappearing:
                {
                    self.BattleVictoryWipeT += self._gameContext.TimeContext.Delta / 2;

                    if (self.BattleVictoryWipeT >= 1.0)
                    {
                        self.BattleVictoryWipeT = 0;
                        return BattleVictoryScreenAutomaton.Result.Stop;
                    }

                    break;
                }
            }

            return BattleVictoryScreenAutomaton.Result.Continue;
        },
    };

    public readonly MugshotStateContext CurrentMugshotStateContext = new(gameContext.TimeContext);

    public string? CurrentPopUpMessage;
    public double BattleVictoryWipeT;
    public float PopUpT;
    public float PopUpWaitT;

    public void ShowPopUp(string message)
    {
        CurrentPopUpMessage = message;
        PopUpStateAutomaton.ChangeState(PopUpState.Appearing);
    }

    public void StartBattleVictoryScreen()
    {
        BattleVictoryStateAutomaton.ChangeState(BattleVictoryScreenState.Appearing);
    }

    public void WipeAwayBattleVictoryScreen()
    {
        BattleVictoryStateAutomaton.ChangeState(BattleVictoryScreenState.Disappearing);
    }

    public void ShakeMugshot()
    {
        CurrentMugshotStateContext.StateAutomaton.ChangeState(MugshotState.Shaking);
    }

    public void Update()
    {
        BattleVictoryStateAutomaton.Update(this);
        PopUpStateAutomaton.Update(this);
        CurrentMugshotStateContext.Update();
    }
}
