namespace Belmondo.FightFightDanger;

public class ViewModel
{
    public float BattleEnemyBillboardShakeT = 0;
    public float BattleEnemyDieT = 0;
    public int BattleEnemyFrame = 0;
    public float ScreenShakeT = 0;

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
    }
}
