namespace Belmondo.FightFightDanger;

public struct Player
{
    public struct Defaults()
    {
        public double WalkCooldown = 1 / 4.0;
        public float Health = 100;
    }

    public event Action? InventoryUpdated;

    public Defaults Default = new();
    public Defaults Current = new();
    public Inventory Inventory = new();
    public float RunningHealth;

    public Player()
    {
        RunningHealth = Default.Health;
    }
}
