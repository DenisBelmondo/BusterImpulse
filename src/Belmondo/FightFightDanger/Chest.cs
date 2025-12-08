namespace Belmondo.FightFightDanger;

public struct Chest
{
    public enum Status
    {
        Idle,
        Opening,
        Opened,
    }

    public required IDictionary<int, int> Items;
    public float CurrentOpenness;
    public Status CurrentStatus;
}
