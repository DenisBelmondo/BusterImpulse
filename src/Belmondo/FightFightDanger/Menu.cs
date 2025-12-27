namespace Belmondo.FightFightDanger;

public class Menu : IResettable
{
    public struct Item
    {
        public string Name;
        public string Description;
    }

    public string? Name;
    public string? Description;
    public int CurrentItem;

    public List<Item> Items = [];

    public void Reset() => CurrentItem = 0;
}
