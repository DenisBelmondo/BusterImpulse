namespace Belmondo.FightFightDanger;

public struct Player()
{
    public struct Defaults()
    {
        public double WalkCooldown = 1 / 4.0;
    }

    public Defaults Default = new();
    public Defaults Current = new();
}
