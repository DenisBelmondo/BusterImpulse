namespace Belmondo.FightFightDanger;

public interface IAudioService
{
    void PlaySoundEffect(SoundEffect soundEffect);
    void ChangeMusic(MusicTrack musicTrack);
    void StopMusic();
}
