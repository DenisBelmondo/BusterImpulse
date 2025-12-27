namespace Belmondo.FightFightDanger;

public static class Names
{
    public static string? Get(Items.Type itemType) => itemType switch
    {
        Items.Type.ChickenLeg => "Chicken Leg",
        Items.Type.WholeChicken => "Whole Chicken",
        _ => null,
    };

    public static string? Get(Charms.Type charmType) => charmType switch
    {
        Charms.Type.ElephantStatue => "Elephant Statue",
        Charms.Type.FourLeafClover => "Four Leaf Clover",
        Charms.Type.GoldenGrapes => "Golden Grapes",
        Charms.Type.Milagro => "Milagro",
        Charms.Type.RabbitsFoot => "Rabbit's Foot",
        Charms.Type.Scarab => "Scarab",
        _ => null,
    };
}
