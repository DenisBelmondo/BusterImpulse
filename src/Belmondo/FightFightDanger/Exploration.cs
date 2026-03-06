namespace Belmondo.FightFightDanger;

public class Exploration
{
    public event Action? BattleRequested;
    public event Action? MainMenuRequested;

    public void Update(in GameContext gameContext, World world)
    {
        var triedToOpenMenu = gameContext.InputService.ActionWasJustPressed(InputAction.Cancel);

        if (gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
        {
            BattleRequested?.Invoke();
        }

        if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
        {
            var newPosition = Direction.ToPosition(world.Player.Transform.Direction);
            var desiredX = world.Player.Transform.Position.X + newPosition.X;
            var desiredY = world.Player.Transform.Position.Y + newPosition.Y;

            TryToInteractWithChest(gameContext, world, new(desiredX, desiredY));
        }

        UpdatePlayer(gameContext, world);
        UpdateChests(gameContext, world);

        if (triedToOpenMenu)
        {
            MainMenuRequested?.Invoke();
        }
    }

    private static void UpdatePlayer(in GameContext gameContext, World world)
    {
        var input = gameContext.InputService;

        if (input.ActionWasJustPressed(InputAction.LookRight))
        {
            world.OldPlayerDirection = world.Player.Transform.Direction;
            world.CameraDirectionLerpT = 0;
            world.Player.Transform.Direction++;
        }
        else if (input.ActionWasJustPressed(InputAction.LookLeft))
        {
            world.OldPlayerDirection = world.Player.Transform.Direction;
            world.CameraDirectionLerpT = 0;
            world.Player.Transform.Direction--;
        }

        world.Player.Transform.Direction = Direction.Clamped(world.Player.Transform.Direction);
        world.CameraDirectionLerpT = MathF.Min(world.CameraDirectionLerpT + (float)gameContext.TimeContext.Delta, 1);

        int? moveDirection = null;

        if (input.ActionIsPressed(InputAction.MoveForward))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction);
        }
        else if (input.ActionIsPressed(InputAction.MoveBack))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction + 2);
        }
        else if (input.ActionIsPressed(InputAction.MoveLeft))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction + 3);
        }
        else if (input.ActionIsPressed(InputAction.MoveRight))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction + 1);
        }

        if (moveDirection is not null && world.Player.Value.Current.WalkCooldown == 0)
        {
            world.CameraPositionLerpT = 0;
            world.OldPlayerX = world.Player.Transform.Position.X;
            world.OldPlayerY = world.Player.Transform.Position.Y;
            gameContext.AudioService.PlaySoundEffect(SoundEffect.Step);
            world.Player.Value.Current.WalkCooldown = world.Player.Value.Default.WalkCooldown;
            world.TraceMove(world.Player.Transform.Position, (int)moveDirection, out world.Player.Transform.Position);
        }

        world.Player.Value.Current.WalkCooldown = Math.Max(
            world.Player.Value.Current.WalkCooldown - gameContext.TimeContext.Delta,
            0);

        world.CameraPositionLerpT = MathF.Min(
            world.CameraPositionLerpT + (
                    (1.0F / (float)world.Player.Value.Default.WalkCooldown)
                    * (float)gameContext.TimeContext.Delta),
            1);
    }

    private static void UpdateChests(in GameContext gameContext, World world)
    {
        for (int i = 0; i < world.Chests.Count; i++)
        {
            var chest = world.Chests[i];

            if (chest.Value.CurrentStatus == Chest.Status.Opening)
            {
                chest.Value.Openness += (float)gameContext.TimeContext.Delta * 2;

                if (chest.Value.Openness >= 1)
                {
                    chest.Value.Openness = 1;
                    chest.Value.CurrentStatus = Chest.Status.Opened;
                    world.OpenChest(i);
                    chest.Value.Inventory.TransferTo(world.Player.Value.Inventory);
                    gameContext.AudioService.PlaySoundEffect(SoundEffect.Item);
                }
            }

            world.Chests[i] = chest;
        }
    }

    private static bool TryToInteractWithChest(in GameContext gameContext, World world, Position at)
    {
        if (!world.ChestMap.TryGetValue(at, out int chestID))
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
}
