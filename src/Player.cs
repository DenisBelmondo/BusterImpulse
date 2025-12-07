namespace Belmondo.FightFightDanger;

public struct Player()
{
    public struct Defaults()
    {
        public double WalkCooldown = 1 / 4.0;
    }

    public Entity Entity;
    public Defaults Default = new();
    public Defaults Current = new();
}
