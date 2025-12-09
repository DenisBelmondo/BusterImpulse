using System.Numerics;

namespace Belmondo.FightFightDanger;

public struct ShakeStateContext(TimeContext timeContext)
{
    public double SecondsLeft;
    public Vector3 Offset;

    public void Shake(double durationSeconds)
    {
        SecondsLeft = durationSeconds;
    }

    public void Update()
    {
        Offset = Vector3.Zero;
        SecondsLeft -= timeContext.Delta;

        if (SecondsLeft > 0)
        {
            Offset = new Vector3(
                Random.Shared.NextSingle() - 0.5f,
                0,
                Random.Shared.NextSingle() - 0.5f
            ) / 10f;
        }
    }
}
