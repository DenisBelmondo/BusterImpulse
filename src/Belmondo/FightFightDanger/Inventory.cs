namespace Belmondo.FightFightDanger;

public class Inventory
{
    public Dictionary<CharmType, int> Charms = [];
    public Dictionary<SnackType, int> Snacks = [];

    public void TransferTo(Inventory other)
    {
        foreach ((var snackType, var quantity) in Snacks)
        {
            if (!other.Snacks.ContainsKey(snackType))
            {
                other.Snacks[snackType] = quantity;
                continue;
            }

            other.Snacks[snackType] += quantity;
        }
    }
}
