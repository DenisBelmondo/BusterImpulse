namespace Belmondo.FightFightDanger;

public static class Items
{
    public static int GetHealAmountForSnack(SnackType snackType) => snackType switch
    {
        SnackType.ChickenLeg => 10,
        SnackType.WholeChicken => 20,
        _ => default,
    };

    public static bool EatSnack(ref Player player, SnackType snackType, Action? onEatAction = null)
    {
        if (!player.Inventory.Snacks.TryGetValue(snackType, out int value))
        {
            return false;
        }

        if (value <= 0)
        {
            return false;
        }

        player.Current.Health += GetHealAmountForSnack(snackType);
        player.Inventory.Snacks[snackType] -= 1;
        onEatAction?.Invoke();

        return true;
    }
}
