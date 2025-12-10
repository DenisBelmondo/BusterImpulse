namespace Belmondo.FightFightDanger;

public class ViewModel
{
    public float BattleEnemyBillboardShakeT;
    public float BattleEnemyDieT;
    public int BattleEnemyFrame;
    public float ScreenShakeT;
    public float PopUpT;
    public float PopUpWaitT;
    public string? CurrentPopUpMessage;

    public required GameContext GameContext;

    public StateAutomaton PopUpStateAutomaton = new();
    public State? PopUpAppearState;
    public State? PopUpStickAroundState;
    public State? PopUpDisappearState;

    public ViewModel()
    {
        PopUpAppearState = new()
        {
            EnterFunction = () =>
            {
                PopUpT = 0;
            },

            UpdateFunction = () =>
            {
                PopUpT += (float)(GameContext?.Delta).GetValueOrDefault() * 5;

                if (PopUpT >= 1)
                {
                    return State.Goto(PopUpStickAroundState!);
                }

                return State.Continue;
            },
        };

        PopUpStickAroundState = new()
        {
            EnterFunction = () =>
            {
                PopUpWaitT = 0;
            },

            UpdateFunction = () =>
            {
                PopUpWaitT += (float)(GameContext?.Delta).GetValueOrDefault();

                if (PopUpWaitT >= 3)
                {
                    return State.Goto(PopUpDisappearState!);
                }

                return State.Continue;
            },
        };

        PopUpDisappearState = new()
        {
            UpdateFunction = () =>
            {
                PopUpT -= (float)(GameContext?.Delta).GetValueOrDefault() * 5;

                if (PopUpT <= 0)
                {
                    return State.Stop;
                }

                return State.Continue;
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

    public void Update(in GameContext gameContext)
    {
        BattleEnemyBillboardShakeT = Math.Max(BattleEnemyBillboardShakeT - ((float)gameContext.Delta), 0);
        BattleEnemyDieT = Math.Max(BattleEnemyDieT - ((float)gameContext.Delta / 2f), 0);
        ScreenShakeT = Math.Max(ScreenShakeT - ((float)gameContext.Delta), 0);

        if (BattleEnemyBillboardShakeT <= 0 && BattleEnemyDieT == 0)
        {
            BattleEnemyFrame = 0;
        }

        PopUpStateAutomaton.Update();
    }
}
