namespace Belmondo;

using Position = (int X, int Y);

public static partial class FightFightDanger
{
    public struct World()
    {
        public struct SpawnedEntity
        {
            public int ID;
            public Entity Entity;
        }

        public int[,]? TileMap;
        public Dictionary<Position, HashSet<int>> BroadPhaseCollisionMap = [];
        public Player Player = new();
        public Dictionary<int, SpawnedEntity> Entities = [];

        public int Spawn(int type, Position at) 
        {
            var id = Entities.Count;

            Entities[id] = new()
            {
                ID = id,
                Entity = new()
                {
                    Type = type,
                    X = at.X,
                    Y = at.Y,
                }
            };

            BroadPhaseCollisionMap[at].Add(id);

            return id;
        }

        public readonly bool TryMove(ref Entity entity, int direction)
        {
            (int X, int Y) = Direction.ToInt32Tuple(direction);

            int desiredX = entity.X + X;
            int desiredY = entity.Y + Y;

            if (TileMap is not null && (TileMap[desiredY, desiredX] != 0))
            {
                return false;
            }

            entity.X = desiredX;
            entity.Y = desiredY;

            return true;
        }
    }
}
