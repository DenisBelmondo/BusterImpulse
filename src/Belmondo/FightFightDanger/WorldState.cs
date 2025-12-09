using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public struct WorldState()
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

    public readonly Dictionary<Position, int> ChestMap = [];

    //
    // entity stuff
    //

    public readonly List<Spawned<ChestState>> Chests = [];
    public Spawned<Player> Player = new(new());

    //
    // running state
    //

    public float OldPlayerX = 0;
    public float OldPlayerY = 0;
    public int OldPlayerDirection = 0;

    public float CameraPositionLerpT = 0;
    public float CameraDirectionLerpT = 0;

    public void SpawnChest(ChestState chest, Transform transform)
    {
        ChestMap.Add(transform.Position, Chests.Count);
        Chests.Add(new()
        {
            ID = Chests.Count,
            Transform = transform,
            Value = chest,
        });
    }
}
