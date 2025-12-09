namespace Belmondo.FightFightDanger;

public struct BattleFoe(BattleStats battleStats)
{
    public readonly BattleStats Default = battleStats;
    public BattleStats Current = battleStats;
}
