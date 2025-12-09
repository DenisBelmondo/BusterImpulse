using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public sealed class World(Services services, TimeContext timeContext)
{
    public struct Spawned<T> where T : struct
    {
        public T Value;
        public Transform Transform;
        public int ID;

        public Spawned(T value) : this() => Value = value;
    }

    //
    // collision stuff
    //

    public int[,]? TileMap;
    public readonly Dictionary<Position, int> ChestMap = [];

    //
    // entity stuff
    //

    public readonly List<Spawned<Chest>> Chests = [];
    public Spawned<Player> Player = new(new());

    //
    // running state
    //

    public float OldPlayerX = 0;
    public float OldPlayerY = 0;
    public int OldPlayerDirection = 0;

    public float CameraPositionLerpT = 0;
    public float CameraDirectionLerpT = 0;

    public bool TryMove(ref Transform entity, int direction)
    {
        var (X, Y) = Direction.ToInt32Tuple(direction);
        int desiredX = entity.X + X;
        int desiredY = entity.Y + Y;

        bool canMoveToDestination = (true
            && (TileMap is not null && (TileMap[desiredY, desiredX] == 0))
            && (!ChestMap.ContainsKey((desiredX, desiredY)))
        );

        if (!canMoveToDestination)
        {
            return false;
        }

        entity.X = desiredX;
        entity.Y = desiredY;

        return true;
    }

    public void SpawnChest(Chest chest, Transform transform)
    {
        ChestMap.Add((transform.X, transform.Y), Chests.Count);
        Chests.Add(new()
        {
            ID = Chests.Count,
            Transform = transform,
            Value = chest,
        });
    }

    public void UpdateChests()
    {
        foreach (ref var spawnedChest in CollectionsMarshal.AsSpan(Chests))
        {
            var chest = spawnedChest.Value;

            if (chest.CurrentStatus == Chest.Status.Opening)
            {
                chest.CurrentOpenness += (float)timeContext.Delta * 2;

                if (chest.CurrentOpenness >= 1)
                {
                    chest.CurrentOpenness = 1;
                }
            }

            spawnedChest.Value = chest;
        }
    }

    public bool TryToInteractWithChest(Position at)
    {
        if (!ChestMap.TryGetValue((at.X, at.Y), out int chestID))
        {
            return false;
        }

        var spawnedChest = Chests[chestID];

        if (spawnedChest.Value.CurrentStatus != Chest.Status.Idle)
        {
            return false;
        }

        spawnedChest.Value.CurrentStatus = Chest.Status.Opening;
        Chests[chestID] = spawnedChest;
        services.AudioService.PlaySoundEffect(SoundEffect.OpenChest);

        return true;
    }
}
