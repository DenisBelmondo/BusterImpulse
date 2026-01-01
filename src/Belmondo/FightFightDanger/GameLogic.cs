namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public static class GameLogic
{
    public static void UpdatePlayer(in GameContext gameContext, World world)
    {
        if (gameContext.InputService.ActionWasJustPressed(InputAction.LookRight))
        {
            world.OldPlayerDirection = world.Player.Transform.Direction;
            world.CameraDirectionLerpT = 0;
            world.Player.Transform.Direction++;
        }
        else if (gameContext.InputService.ActionWasJustPressed(InputAction.LookLeft))
        {
            world.OldPlayerDirection = world.Player.Transform.Direction;
            world.CameraDirectionLerpT = 0;
            world.Player.Transform.Direction--;
        }

        world.Player.Transform.Direction = Direction.Clamped(world.Player.Transform.Direction);
        world.CameraDirectionLerpT = MathF.Min(world.CameraDirectionLerpT + (float)gameContext.TimeContext.Delta, 1);

        int? moveDirection = null;

        if (gameContext.InputService.ActionIsPressed(InputAction.MoveForward))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction);
        }
        else if (gameContext.InputService.ActionIsPressed(InputAction.MoveBack))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction + 2);
        }
        else if (gameContext.InputService.ActionIsPressed(InputAction.MoveLeft))
        {
            moveDirection = Direction.Clamped(world.Player.Transform.Direction + 3);
        }
        else if (gameContext.InputService.ActionIsPressed(InputAction.MoveRight))
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
            world.TryMove(world.Player.Transform.Position, (int)moveDirection, out world.Player.Transform.Position);
        }

        world.Player.Value.Current.WalkCooldown = Math.Max(
            world.Player.Value.Current.WalkCooldown - gameContext.TimeContext.Delta,
            0);

        world.CameraPositionLerpT = MathF.Min(
            world.CameraPositionLerpT
                + ((1.0F / (float)world.Player.Value.Default.WalkCooldown)
                * (float)gameContext.TimeContext.Delta),
            1);
    }

    public static void UpdateChests(in GameContext gameContext, World world)
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
}
