using System.Numerics;

namespace Belmondo.FightFightDanger;

using PopUpStateAutomaton = StateAutomaton<UIState, UIState.PopUpState>;
using PopUpStateResult = StateFlowResult<UIState.PopUpState>;
using BattleVictoryScreenAutomaton = StateAutomaton<UIState, UIState.BattleVictoryScreenState>;
using BattleVictoryScreenStateResult = StateFlowResult<UIState.BattleVictoryScreenState>;
using MugshotStateAutomaton = StateAutomaton<UIState.MugshotStateContext, UIState.MugshotState>;
using MugshotStateResult = StateFlowResult<UIState.MugshotState>;

public class UIState(GameContext gameContext)
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

    public class MugshotStateContext(TimeContext timeContext)
    {
        private readonly TimeContext _timeContext = timeContext;
        public readonly Timer ShakeTimer = new();

        public readonly MugshotStateAutomaton StateAutomaton = new()
        {
            EnterFunction = static (self, currentState) =>
            {
                self.ShakeTimer.Start(0.25);
                return MugshotStateResult.Continue;
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
                    return MugshotStateResult.Stop;
                }

                return MugshotStateResult.Continue;
            },

            ExitFunction = static (self, currentState) =>
            {
                if (currentState == MugshotState.Shaking)
                {
                    self.ShakeOffset = Vector2.Zero;
                }

                return MugshotStateResult.Continue;
            },
        };

        public Vector2 ShakeOffset;

        public void Update()
        {
            StateAutomaton.Update(this);
            ShakeTimer.Update(_timeContext);
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

            return PopUpStateResult.Continue;
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
                        return PopUpStateResult.Goto(PopUpState.StickingAround);
                    }

                    break;
                }

                case PopUpState.StickingAround:
                {
                    self.PopUpWaitT += (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault();

                    if (self.PopUpWaitT >= 3)
                    {
                        return PopUpStateResult.Goto(PopUpState.Disappearing);
                    }

                    break;
                }

                case PopUpState.Disappearing:
                {
                    self.PopUpT -= (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                    if (self.PopUpT <= 0)
                    {
                        return PopUpStateResult.Stop;
                    }

                    break;
                }
            }

            return PopUpStateResult.Continue;
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

            return BattleVictoryScreenStateResult.Continue;
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
                        return BattleVictoryScreenStateResult.Stop;
                    }

                    break;
                }

                case BattleVictoryScreenState.Disappearing:
                {
                    self.BattleVictoryWipeT += self._gameContext.TimeContext.Delta / 2;

                    if (self.BattleVictoryWipeT >= 1.0)
                    {
                        self.BattleVictoryWipeT = 0;
                        return BattleVictoryScreenStateResult.Stop;
                    }

                    break;
                }
            }

            return BattleVictoryScreenStateResult.Continue;
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
