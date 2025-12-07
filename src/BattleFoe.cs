namespace Belmondo;

public static partial class FightFightDanger
{
    public sealed class BattleFoe(BattleFoe.Stats stats)
    {
        public struct Stats
        {
            public int Health;
            public int Damage;
        }

        public readonly Stats Default = stats;
        public Stats Current = stats;
    }
}
