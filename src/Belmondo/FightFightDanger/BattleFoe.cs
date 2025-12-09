namespace Belmondo.FightFightDanger;

public struct BattleFoe(BattleFoe.Stats stats)
{
    public struct Stats
    {
        public int Health;
        public int Damage;
    }

    public readonly Stats Default = stats;
    public Stats Current = stats;
}
