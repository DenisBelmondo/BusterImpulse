using System.Numerics;

namespace Belmondo.FightFightDanger;

public static class Math2
{
    public static float Lerp(float v0, float v1, float t) => (1 - t) * v0 + t * v1;

    public static float SampleParabola(float x, float a, float b, float c)
    {
        return (a * x * x) + (b * x) + c;
    }

    public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
        );
    }

    public static void CreateSpline(Vector3[] controlPoints, int segmentCount, in Span<Vector3> outSplinePoints)
    {
        int segsLimit = Math.Min(segmentCount, outSplinePoints.Length);

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 p0 = (i == 0) ? controlPoints[0] : controlPoints[i - 1];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = (i + 2 < controlPoints.Length) ? controlPoints[i + 2] : controlPoints[i + 1];

            for (int j = 0; j < segsLimit; j++)
            {
                float t = j / (float)segmentCount;
                outSplinePoints[j] = CatmullRom(p0, p1, p2, p3, t);
            }
        }
    }

    public static Vector3 SampleCatmullRom(Vector3[] controlPoints, float t)
    {
        // Ensure t is within [0, 1]
        t = Math.Clamp(t, 0, 1);

        // Determine the segment to use based on t
        int segmentCount = controlPoints.Length - 1;
        float segmentLength = 1f / segmentCount;
        int segmentIndex = (int)(t / segmentLength);

        // Ensure that the segmentIndex is within bounds
        segmentIndex = Math.Clamp(segmentIndex, 0, segmentCount - 1);

        // Compute the local t for the segment
        float localT = (t - segmentIndex * segmentLength) / segmentLength;

        // Get the control points for the current segment
        Vector3 p0 = (segmentIndex == 0) ? controlPoints[0] : controlPoints[segmentIndex - 1];
        Vector3 p1 = controlPoints[segmentIndex];
        Vector3 p2 = controlPoints[segmentIndex + 1];
        Vector3 p3 = (segmentIndex + 2 < controlPoints.Length) ? controlPoints[segmentIndex + 2] : controlPoints[segmentIndex + 1];

        // Return the sampled point from the Catmull-Rom spline
        return CatmullRom(p0, p1, p2, p3, localT);
    }
}
