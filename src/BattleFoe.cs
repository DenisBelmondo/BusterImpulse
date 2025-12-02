namespace Belmondo;

public static partial class FightFightDanger
{
    public partial class BattleFoe(BattleFoe.Stats stats)
    {
        public partial struct Stats
        {
            public int Health;
            public int Damage;
        }

        public readonly Stats Default = stats;
        public Stats Current = stats;
    }

    public partial class BattleFoe
    {
        public partial struct Stats
        {
            public static readonly Stats Goon = new()
            {
                Health = 5,
                Damage = 1,
            };
        }
    }
}
