using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo;

public static partial class FightFightDanger
{
    public sealed class World
    {
        public struct SpawnedEntity
        {
            public int ID;
            public Entity Entity;
        }

        //
        // collision stuff
        //
        public int[,]? TileMap;
        public int[,]? BroadPhaseCollisionMap;

        //
        // entity stuff
        //
        public Player Player = new();
        public List<Chest> Chests = [];

        //
        // running state
        //

        public float OldPlayerX = 0;
        public float OldPlayerY = 0;
        public int OldPlayerDirection = 0;

        public float CameraPositionLerpT = 0;
        public float CameraDirectionLerpT = 0;

        public bool TryMove(ref Entity entity, int direction)
        {
            var (X, Y) = Direction.ToInt32Tuple(direction);
            int desiredX = entity.X + X;
            int desiredY = entity.Y + Y;

            bool canMoveToDestination = (true
                && (TileMap is not null && (TileMap[desiredY, desiredX] == 0))
            );

            if (!canMoveToDestination)
            {
                return false;
            }

            entity.X = desiredX;
            entity.Y = desiredY;

            return true;
        }

        public void UpdatePlayer(double delta)
        {
            // begin player update

            if (IsKeyPressed(KeyboardKey.Right))
            {
                OldPlayerDirection = Player.Entity.Direction;
                CameraDirectionLerpT = 0;
                Player.Entity.Direction++;
            }
            else if (IsKeyPressed(KeyboardKey.Left))
            {
                OldPlayerDirection = Player.Entity.Direction;
                CameraDirectionLerpT = 0;
                Player.Entity.Direction--;
            }

            Player.Entity.Direction = Direction.Clamped(Player.Entity.Direction);
            CameraDirectionLerpT = MathF.Min(CameraDirectionLerpT + (float)delta, 1);

            int? moveDirection = null;

            if (IsKeyDown(KeyboardKey.W))
            {
                moveDirection = Direction.Clamped(Player.Entity.Direction);
            }
            else if (IsKeyDown(KeyboardKey.S))
            {
                moveDirection = Direction.Clamped(Player.Entity.Direction + 2);
            }
            else if (IsKeyDown(KeyboardKey.A))
            {
                moveDirection = Direction.Clamped(Player.Entity.Direction + 3);
            }
            else if (IsKeyDown(KeyboardKey.D))
            {
                moveDirection = Direction.Clamped(Player.Entity.Direction + 1);
            }

            if (moveDirection is not null && Player.Current.WalkCooldown == 0)
            {
                CameraPositionLerpT = 0;
                OldPlayerX = Player.Entity.X;
                OldPlayerY = Player.Entity.Y;
                PlaySound(Resources.StepSound);
                Player.Current.WalkCooldown = Player.Default.WalkCooldown;
                TryMove(ref Player.Entity, (int)moveDirection);
            }

            Player.Current.WalkCooldown = Math.Max(Player.Current.WalkCooldown - delta, 0);
            CameraPositionLerpT = MathF.Min(CameraPositionLerpT + ((1.0F / (float)Player.Default.WalkCooldown) * (float)delta), 1);

            // end player update
        }
    }
}
