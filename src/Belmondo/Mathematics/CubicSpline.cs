using System.Numerics;

namespace Belmondo.Mathematics;

//
// freakin AI slop...
//
public static class CubicSpline
{
    public static Vector3 Sample(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result = new Vector3(
            0.5f * ((2 * p1.X) +
                    (-p0.X + p2.X) * t +
                    (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                    (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3),
            0.5f * ((2 * p1.Y) +
                    (-p0.Y + p2.Y) * t +
                    (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                    (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3),
            0.5f * ((2 * p1.Z) +
                    (-p0.Z + p2.Z) * t +
                    (2 * p0.Z - 5 * p1.Z + 4 * p2.Z - p3.Z) * t2 +
                    (-p0.Z + 3 * p1.Z - 3 * p2.Z + p3.Z) * t3)
        );

        return result;
    }
}
