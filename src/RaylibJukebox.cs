using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo;

public static partial class FightFightDanger
{
	public sealed partial class RaylibJukebox
	{
		public Music Music;

		public void Update()
		{
			UpdateMusicStream(Music);
		}
	}

    public sealed partial class RaylibJukebox : IJukebox<Music>
    {
        public void ChangeMusic(Music music)
        {
			unsafe
			{
				if (music.Stream.Buffer == Music.Stream.Buffer)
				{
					return;
				}
			}

			StopMusicStream(Music);
			Music = music;
			PlayMusicStream(Music);
        }
    }
}
