namespace Belmondo;

using Position = (int X, int Y);

public static partial class FightFightDanger
{
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
}
