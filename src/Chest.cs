namespace Belmondo.FightFightDanger;

public struct Chest
{
    public required IDictionary<int, int> Items;
    public Entity Entity;
    public float CurrentOpenness;

    public readonly int GetItemQuantity(int type)
    {
        Items.TryGetValue(type, out int quantity);
        return quantity;
    }

    public void Update(float delta)
    {
        CurrentOpenness += delta;
    }
}
