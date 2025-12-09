namespace Belmondo.FightFightDanger;

public struct BattleScreenWipeState
{
    public float T;
}

public struct BattlePlayerAimingState
{
    public (double, double) CurrentRange;
    public double CurrentAimValue;

    public readonly bool IsInRange() => CurrentAimValue >= CurrentRange.Item1 && CurrentAimValue <= CurrentRange.Item2;
}

public struct BattleState
{
    public BattleScreenWipeState ScreenWipeState;
    public BattlePlayerAimingState PlayerAimingState;
    public BattleFoe Foe;
}
