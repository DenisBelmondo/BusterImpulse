namespace Belmondo.FightFightDanger;

public static class Names
{
    public static string Get(SnackType snackType) => snackType switch
    {
        SnackType.ChickenLeg => "Chicken Leg",
        SnackType.WholeChicken => "Whole Chicken",
        _ => snackType.ToString(),
    };

    public static string Get(CharmType charmType) => charmType switch
    {
        CharmType.ElephantStatue => "Elephant Statue",
        CharmType.FourLeafClover => "Four Leaf Clover",
        CharmType.GoldenGrapes => "Golden Grapes",
        CharmType.Milagro => "Milagro",
        CharmType.RabbitsFoot => "Rabbit's Foot",
        CharmType.Scarab => "Scarab",
        _ => charmType.ToString(),
    };
}
