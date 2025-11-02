using System.Numerics;

namespace Belmondo;

public static partial class FightFightDanger
{
	public struct ShakeStateContext
	{
		public double SecondsLeft;
		public Vector3 Offset;
		public State State;

		public void Shake(double durationSeconds)
		{
			SecondsLeft = durationSeconds;
		}

		public void Update(double delta)
		{
			Offset = Vector3.Zero;
			SecondsLeft -= delta;

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
}
