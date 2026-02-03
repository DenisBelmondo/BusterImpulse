namespace Belmondo.FightFightDanger;

public struct Item
{
    public ItemType Type;
    public SnackType? Snack;
    public CharmType? CharmType;

    public static Item NewSnack(SnackType snackType) => new()
    {
        Type = ItemType.Snack,
        Snack = snackType,
    };

    public static Item NewCharm(CharmType charmType) => new()
    {
        Type = ItemType.Charm,
        CharmType = charmType,
    };
}
