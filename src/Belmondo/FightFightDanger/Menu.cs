namespace Belmondo.FightFightDanger;

using ResetFlags = Menu.ResetFlags;

public class Menu : IResettable, IResettable<ResetFlags>
{
    [Flags]
    public enum ResetFlags
    {
        CurrentItem = 1 << 0,
        AllItems    = 1 << 1,
    }

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
    public List<Item> Items = [];
    public int CurrentItem;

    public void Reset() => Reset(ResetFlags.CurrentItem);

    public void Reset(ResetFlags flags)
    {
        if ((flags & ResetFlags.CurrentItem) != 0)
        {
            CurrentItem = 0;
        }

        if ((flags & ResetFlags.AllItems) != 0)
        {
            Items.Clear();
        }
    }
}
