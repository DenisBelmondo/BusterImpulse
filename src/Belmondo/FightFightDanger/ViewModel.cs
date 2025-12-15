namespace Belmondo.FightFightDanger;

public class ViewModel(GameContext gameContext)
{
    public static StateAutomaton<ViewModel> PopUpStateAutomaton = new();
    public static State<ViewModel> PopUpAppearState;
    public static State<ViewModel> PopUpStickAroundState;
    public static State<ViewModel> PopUpDisappearState;

    private readonly GameContext _gameContext = gameContext;

    public float BattleEnemyBillboardShakeT;
    public float BattleEnemyDieT;
    public int BattleEnemyFrame;
    public float ScreenShakeT;
    public float PopUpT;
    public float PopUpWaitT;
    public string? CurrentPopUpMessage;

    static ViewModel()
    {
        PopUpAppearState = new()
        {
            EnterFunction = self =>
            {
                self.PopUpT = 0;
            },

            UpdateFunction = self =>
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
            EnterFunction = self =>
            {
                self.PopUpWaitT = 0;
            },

            UpdateFunction = self =>
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
            UpdateFunction = self =>
            {
                self.PopUpT -= (float)(self._gameContext?.TimeContext.Delta).GetValueOrDefault() * 5;

                if (self.PopUpT <= 0)
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
