namespace Belmondo.FightFightDanger;

public static class Names
{
    public static string Get(SnackType snackType) => snackType switch
    {
        SnackType.ChickenLeg => "Chicken Leg",
        SnackType.WholeChicken => "Whole Chicken",
        _ => snackType.ToString(),
    };

    public static string Get(Charms.Type charmType) => charmType switch
    {
        Charms.Type.ElephantStatue => "Elephant Statue",
        Charms.Type.FourLeafClover => "Four Leaf Clover",
        Charms.Type.GoldenGrapes => "Golden Grapes",
        Charms.Type.Milagro => "Milagro",
        Charms.Type.RabbitsFoot => "Rabbit's Foot",
        Charms.Type.Scarab => "Scarab",
        _ => charmType.ToString(),
    };
}
