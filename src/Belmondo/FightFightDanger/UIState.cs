namespace Belmondo.FightFightDanger;

public class UIState(GameContext gameContext) : IThinker
{
    public static State<UIState> BattleVictoryScreenEnterState = State<UIState>.Empty;
    public static State<UIState> BattleVictoryScreenExitState = State<UIState>.Empty;

    public static State<UIState> PopUpAppearState = State<UIState>.Empty;
    public static State<UIState> PopUpStickAroundState = State<UIState>.Empty;
    public static State<UIState> PopUpDisappearState = State<UIState>.Empty;

    private readonly GameContext _gameContext = gameContext;
    public readonly StateAutomaton<UIState> PopUpStateAutomaton = new();
    public readonly StateAutomaton<UIState> BattleVictoryStateAutomaton = new();
    public readonly StateAutomaton<UIState> BattleVictoryLabelsStateAutomaton = new();

    public string? CurrentPopUpMessage;
    public double BattleVictoryWipeT;
    public float PopUpT;
    public float PopUpWaitT;

    static UIState()
    {
        BattleVictoryScreenEnterState = new()
        {
            EnterFunction = static self =>
            {
                self.BattleVictoryWipeT = 0;
            },

            UpdateFunction = static self =>
            {
                self.BattleVictoryWipeT += self._gameContext.TimeContext.Delta / 2;

                if (self.BattleVictoryWipeT >= 0.5)
                {
                    self.BattleVictoryWipeT = 0.5;
                    return State<UIState>.Stop;
                }

                return State<UIState>.Continue;
            },
        };

        BattleVictoryScreenExitState = new()
        {
            EnterFunction = static self =>
            {
                self.BattleVictoryWipeT = 0.5;
            },

            UpdateFunction = static self =>
            {
                self.BattleVictoryWipeT += self._gameContext.TimeContext.Delta / 2;

                if (self.BattleVictoryWipeT >= 1.0)
                {
                    self.BattleVictoryWipeT = 0;
                    return State<UIState>.Stop;
                }

                return State<UIState>.Continue;
            },
        };

        PopUpAppearState = new()
        {
            EnterFunction = static self =>
            {
                self.PopUpT = 0;
            },

            UpdateFunction = static self =>
            {
                self.PopUpT += (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                if (self.PopUpT >= 1)
                {
                    return State<UIState>.Goto(PopUpStickAroundState!);
                }

                return State<UIState>.Continue;
            },
        };

        PopUpStickAroundState = new()
        {
            EnterFunction = static self =>
            {
                self.PopUpWaitT = 0;
            },

            UpdateFunction = static self =>
            {
                self.PopUpWaitT += (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault();

                if (self.PopUpWaitT >= 3)
                {
                    return State<UIState>.Goto(PopUpDisappearState!);
                }

                return State<UIState>.Continue;
            },
        };

        PopUpDisappearState = new()
        {
            UpdateFunction = static self =>
            {
                self.PopUpT -= (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                if (self.PopUpT <= 0)
                {
                    return State<UIState>.Stop;
                }

                return State<UIState>.Continue;
            },
        };
    }

    public void ShowPopUp(string message)
    {
        CurrentPopUpMessage = message;
        PopUpStateAutomaton.CurrentState = PopUpAppearState;
    }

    public void StartBattleVictoryScreen()
    {
        BattleVictoryStateAutomaton.ChangeState(BattleVictoryScreenEnterState);
    }

    public void WipeAwayBattleVictoryScreen()
    {
        BattleVictoryStateAutomaton.ChangeState(BattleVictoryScreenExitState);
    }

    public void Update()
    {
        BattleVictoryStateAutomaton.Update(this);
        PopUpStateAutomaton.Update(this);
    }
}
