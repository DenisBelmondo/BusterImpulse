namespace Belmondo.FightFightDanger;

public class ViewModel(GameContext gameContext)
{
    public static StateAutomaton<ViewModel> PopUpStateAutomaton = new();
    public static State<ViewModel> PopUpAppearState = State<ViewModel>.Empty;
    public static State<ViewModel> PopUpStickAroundState = State<ViewModel>.Empty;
    public static State<ViewModel> PopUpDisappearState = State<ViewModel>.Empty;

    public static StateAutomaton<ViewModel> TransitionStateAutomaton = new();
    public static State<ViewModel> TransitionFadeInState = State<ViewModel>.Empty;
    public static State<ViewModel> TransitionFadeOutState = State<ViewModel>.Empty;

    private readonly GameContext _gameContext = gameContext;

    public float BattleEnemyBillboardShakeT;
    public float BattleEnemyDieT;
    public int BattleEnemyFrame;
    public float ScreenShakeT;
    public float PopUpT;
    public float PopUpWaitT;
    public string? CurrentPopUpMessage;
    public float ScreenTransitionT;

    static ViewModel()
    {
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
                    return State<ViewModel>.Goto(PopUpStickAroundState!);
                }

                return State<ViewModel>.Continue;
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
                    return State<ViewModel>.Goto(PopUpDisappearState!);
                }

                return State<ViewModel>.Continue;
            },
        };

        PopUpDisappearState = new()
        {
            UpdateFunction = static self =>
            {
                self.PopUpT -= (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                if (self.PopUpT <= 0)
                {
                    return State<ViewModel>.Stop;
                }

                return State<ViewModel>.Continue;
            },
        };

        TransitionFadeInState = new()
        {
            UpdateFunction = static self =>
            {
                self.ScreenTransitionT += (float)self._gameContext.TimeContext.Delta;

                if (self.ScreenTransitionT >= 1f)
                {
                    return State<ViewModel>.Stop;
                }

                return State<ViewModel>.Continue;
            },
        };

        TransitionFadeOutState = new()
        {
            UpdateFunction = static self =>
            {
                self.ScreenTransitionT += (float)self._gameContext.TimeContext.Delta;

                if (self.ScreenTransitionT >= 2f)
                {
                    return State<ViewModel>.Stop;
                }

                return State<ViewModel>.Continue;
            },
        };
    }

    public void ShowPopUp(string message)
    {
        CurrentPopUpMessage = message;
        PopUpStateAutomaton.CurrentState = PopUpAppearState;
    }

    public void ShakeBattleEnemy()
    {
        BattleEnemyBillboardShakeT = 0.25f;
        BattleEnemyFrame = 2;
    }

    public void KillBattleEnemy()
    {
        BattleEnemyDieT = 1f;
        BattleEnemyFrame = 3;
    }

    public void ShakeScreen()
    {
        ScreenShakeT = 0.25f;
    }

    public void Update()
    {
        BattleEnemyBillboardShakeT = Math.Max(BattleEnemyBillboardShakeT - ((float)_gameContext.TimeContext.Delta), 0);
        BattleEnemyDieT = Math.Max(BattleEnemyDieT - ((float)_gameContext.TimeContext.Delta / 2f), 0);
        ScreenShakeT = Math.Max(ScreenShakeT - ((float)_gameContext.TimeContext.Delta), 0);

        if (BattleEnemyBillboardShakeT <= 0 && BattleEnemyDieT == 0)
        {
            BattleEnemyFrame = 0;
        }

        PopUpStateAutomaton.Update(this);
    }
}
