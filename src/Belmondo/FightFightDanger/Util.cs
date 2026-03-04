namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public static class Util
{
    public static readonly int SnackTypeCount = Enum.GetNames<SnackType>().Length;
    public static readonly int CharmTypeCount = Enum.GetNames<CharmType>().Length;

    public static bool TryToInteractWithChest(in GameContext gameContext, World world, Position at)
    {
        if (!world.ChestMap.TryGetValue((at.X, at.Y), out int chestID))
        {
            return false;
        }

        var spawnedChest = world.Chests[chestID];

        if (spawnedChest.Value.CurrentStatus != Chest.Status.Idle)
        {
            return false;
        }

        spawnedChest.Value.CurrentStatus = Chest.Status.Opening;
        world.Chests[chestID] = spawnedChest;
        gameContext.AudioService.PlaySoundEffect(SoundEffect.OpenChest);

        return true;
    }

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

        switch (snackType)
        {
            case SnackType.ChickenLeg:
                player.Current.Health += 10;
                break;

            case SnackType.WholeChicken:
                player.Current.Health += 20;
                break;
        }

        player.Inventory.Snacks[snackType] -= 1;
        onEatAction?.Invoke();

        return true;
    }
}

public static class Direction
{
    public const int North = 0;
    public const int East = 1;
    public const int South = 2;
    public const int West = 3;

    public static int Clamped(int direction)
    {
        if (direction < 0)
        {
            direction += 4;
        }

        return direction % 4;
    }

    public static Position ToInt32Tuple(int direction) => direction switch
    {
        North => (0, -1),
        East => (1, 0),
        South => (0, 1),
        West => (-1, 0),
        _ => (0, 0),
    };
}
