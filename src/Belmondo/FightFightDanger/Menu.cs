namespace Belmondo.FightFightDanger;

public class Menu : IResettable
{
    public struct Item
    {
        public int ID;
        public string Label;
        public string Description;
        public int? Quantity;
    }

    public int ID;
    public string? Label;
    public string? Description;
    public int CurrentItem;

    public List<Item> Items = [];

    public void Reset() => CurrentItem = 0;
}
