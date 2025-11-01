namespace Belmondo;

public static partial class FightFightDanger
{
	public interface IJukebox<TMusic>
	{
		void ChangeMusic(TMusic music);
	}
}
