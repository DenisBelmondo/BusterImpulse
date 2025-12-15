using System.Numerics;

namespace Belmondo.Mathematics;

public static class Extensions
{
    public static float SignedAngleTo(this Vector3 v1, Vector3 v2, Vector3 up)
    {
        float angle = MathF.Acos(Vector3.Dot(Vector3.Normalize(v1), Vector3.Normalize(v2)));
        var cross = Vector3.Cross(v1, v2);

        if (Vector3.Dot(up, cross) < 0)
        {
            angle = -angle;
        }

        return angle;
    }
}
