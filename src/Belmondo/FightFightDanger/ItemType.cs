namespace Belmondo.FightFightDanger;

public enum ItemType
{
    ChickenLeg,
    WholeChicken,
}

public static class ItemTypeNames
{
    public static string? Get(ItemType itemType) => itemType switch
    {
        ItemType.ChickenLeg => "Chicken Leg",
        ItemType.WholeChicken => "Whole Chicken",
        _ => null,
    };
}
