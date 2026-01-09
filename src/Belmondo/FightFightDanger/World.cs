namespace Belmondo.FightFightDanger;

using Position = (int X, int Y);

public class World
{
    public struct Spawned<T> where T : struct
    {
        public T Value;
        public Transform Transform;
        public int ID;

        public Spawned(T value) : this() => Value = value;
    }

    public event Action<int>? ChestOpened;

    //
    // collision stuff
    //

    public readonly Dictionary<Position, int> ChestMap = [];

    //
    // entity stuff
    //

    public readonly List<Spawned<Chest>> Chests = [];

    // [TODO]: allow this to be dependency injected from the outside cuz we
    // probably want the player to exist outside just the world.
    public Spawned<Player> Player = new(new());

    //
    // running state
    //

    public float OldPlayerX = 0;
    public float OldPlayerY = 0;
    public int OldPlayerDirection = 0;

    public float CameraPositionLerpT = 0;
    public float CameraDirectionLerpT = 0;

    public void SpawnChest(Transform transform)
    {
        ChestMap.Add(transform.Position, Chests.Count);

        Chests.Add(new()
        {
            ID = Chests.Count,
            Transform = transform,
            Value = new(),
        });
    }

    public int[,]? TileMap;

    public void Initialize(in Map map)
    {
        TileMap = new int[map.Walls.GetLength(0), map.Walls.GetLength(1)];

        for (int y = 0; y < map.Walls.GetLength(0); y++)
        {
            for (int x = 0; x < map.Walls.GetLength(1); x++)
            {
                TileMap[y, x] = map.Walls[y, x].Type;
            }
        }
    }

    public bool TryMove(in Position from, int direction, out Position result)
    {
        var (X, Y) = Direction.ToInt32Tuple(direction);
        int desiredX = from.X + X;
        int desiredY = from.Y + Y;

        result = from;

        bool canMoveToDestination = (true
            && (TileMap is not null && (TileMap[desiredY, desiredX] == 0))
            && (!ChestMap.ContainsKey((desiredX, desiredY)))
        );

        if (!canMoveToDestination)
        {
            return false;
        }

        result.X = desiredX;
        result.Y = desiredY;

        return true;
    }

    public void OpenChest(int chestID) => ChestOpened?.Invoke(chestID);
}
