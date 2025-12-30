namespace Belmondo.FightFightDanger;

public struct Item
{
    public ItemType Type;
    public SnackType? Snack;
    public Charms.Type? CharmType;

    public static Item NewSnack(SnackType snackType) => new()
    {
        Type = ItemType.Snack,
        Snack = snackType,
    };

    public static Item NewCharm(Charms.Type charmType) => new()
    {
        Type = ItemType.Charm,
        CharmType = charmType,
    };
}
