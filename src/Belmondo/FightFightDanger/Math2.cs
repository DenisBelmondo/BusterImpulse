namespace Belmondo.FightFightDanger;

public static class Math2
{
    public static float Lerp(float v0, float v1, float t) => (1 - t) * v0 + t * v1;

    public static float SampleParabola(float x, float a, float b, float c)
    {
        return (a * x * x) + (b * x) + c;
    }
}
