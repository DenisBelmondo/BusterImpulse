namespace Belmondo.FightFightDanger;

public enum ChestStatus
{
    Idle,
    Opening,
    Opened,
}

public struct Chest
{
    public required IDictionary<int, int> Items;
    public float Openness;
    public ChestStatus Status;
}
