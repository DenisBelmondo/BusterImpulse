namespace Belmondo.FightFightDanger;

public struct Chest
{
    public required IDictionary<int, int> Items;
    public ChestState Current;
}
