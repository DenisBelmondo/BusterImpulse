using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public sealed class World(Services services, TimeContext timeContext)
{
    public int[,]? TileMap;

    public bool TryMove(ref WorldState worldState, in Position from, int direction, out Position result)
    {
        var (X, Y) = Direction.ToInt32Tuple(direction);
        int desiredX = from.X + X;
        int desiredY = from.Y + Y;

        result = from;

        bool canMoveToDestination = (true
            && (TileMap is not null && (TileMap[desiredY, desiredX] == 0))
            && (!worldState.ChestMap.ContainsKey((desiredX, desiredY)))
        );

        if (!canMoveToDestination)
        {
            return false;
        }

        result.X = desiredX;
        result.Y = desiredY;

        return true;
    }

    public void UpdatePlayer(ref WorldState worldState)
    {
        if (services.InputService.ActionWasJustPressed(InputAction.LookRight))
        {
            worldState.OldPlayerDirection = worldState.Player.Transform.Direction;
            worldState.CameraDirectionLerpT = 0;
            worldState.Player.Transform.Direction++;
        }
        else if (services.InputService.ActionWasJustPressed(InputAction.LookLeft))
        {
            worldState.OldPlayerDirection = worldState.Player.Transform.Direction;
            worldState.CameraDirectionLerpT = 0;
            worldState.Player.Transform.Direction--;
        }

        worldState.Player.Transform.Direction = Direction.Clamped(worldState.Player.Transform.Direction);
        worldState.CameraDirectionLerpT = MathF.Min(worldState.CameraDirectionLerpT + (float)timeContext.Delta, 1);

        int? moveDirection = null;

        if (services.InputService.ActionIsPressed(InputAction.MoveForward))
        {
            moveDirection = Direction.Clamped(worldState.Player.Transform.Direction);
        }
        else if (services.InputService.ActionIsPressed(InputAction.MoveBack))
        {
            moveDirection = Direction.Clamped(worldState.Player.Transform.Direction + 2);
        }
        else if (services.InputService.ActionIsPressed(InputAction.MoveLeft))
        {
            moveDirection = Direction.Clamped(worldState.Player.Transform.Direction + 3);
        }
        else if (services.InputService.ActionIsPressed(InputAction.MoveRight))
        {
            moveDirection = Direction.Clamped(worldState.Player.Transform.Direction + 1);
        }

        if (moveDirection is not null && worldState.Player.Value.Current.WalkCooldown == 0)
        {
            worldState.CameraPositionLerpT = 0;
            worldState.OldPlayerX = worldState.Player.Transform.Position.X;
            worldState.OldPlayerY = worldState.Player.Transform.Position.Y;
            services.AudioService.PlaySoundEffect(SoundEffect.Step);
            worldState.Player.Value.Current.WalkCooldown = worldState.Player.Value.Default.WalkCooldown;
            TryMove(ref worldState, worldState.Player.Transform.Position, (int)moveDirection, out worldState.Player.Transform.Position);
        }

        worldState.Player.Value.Current.WalkCooldown = Math.Max(
            worldState.Player.Value.Current.WalkCooldown - timeContext.Delta,
            0);

        worldState.CameraPositionLerpT = MathF.Min(
            worldState.CameraPositionLerpT
                + ((1.0F / (float)worldState.Player.Value.Default.WalkCooldown)
                * (float)timeContext.Delta),
            1);
    }

    public void UpdateChests(ref WorldState worldState)
    {
        foreach (ref var spawnedChest in CollectionsMarshal.AsSpan(worldState.Chests))
        {
            var chest = spawnedChest.Value;

            if (chest.Current.Status == ChestStatus.Opening)
            {
                chest.Current.Openness += (float)timeContext.Delta * 2;

                if (chest.Current.Openness >= 1)
                {
                    chest.Current.Openness = 1;
                }
            }

            spawnedChest.Value = chest;
        }
    }

    public bool TryToInteractWithChest(ref WorldState worldState, Position at)
    {
        if (!worldState.ChestMap.TryGetValue((at.X, at.Y), out int chestID))
        {
            return false;
        }

        var spawnedChest = worldState.Chests[chestID];

        if (spawnedChest.Value.Current.Status != ChestStatus.Idle)
        {
            return false;
        }

        spawnedChest.Value.Current.Status = ChestStatus.Opening;
        worldState.Chests[chestID] = spawnedChest;
        services.AudioService.PlaySoundEffect(SoundEffect.OpenChest);

        return true;
    }
}
