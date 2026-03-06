using System.Numerics;

namespace Belmondo.FightFightDanger;

public static class Util
{
    public static readonly int SnackTypeCount = Enum.GetNames<SnackType>().Length;
    public static readonly int CharmTypeCount = Enum.GetNames<CharmType>().Length;

    public static bool SpheresAreColliding(Vector3 p1, float radius1, Vector3 p2, float radius2)
        => Vector3.Distance(p1, p2) < radius1 + radius2;
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

    public static Position ToPosition(int direction) => direction switch
    {
        North => new(0, -1),
        East => new(1, 0),
        South => new(0, 1),
        West => new(-1, 0),
        _ => new(0, 0),
    };
}
