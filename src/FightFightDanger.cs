namespace Belmondo;

public static partial class FightFightDanger
{
	public enum EntityType
	{
		Enemy,
		EnemyGoon,
		EnemyEnd,
	}

	public static class Entities
	{
		public static bool IsEnemy(int entityType)
		{
			var e = (EntityType)entityType;
			return e >= EntityType.Enemy && e < EntityType.EnemyEnd;
		}
	}
}
