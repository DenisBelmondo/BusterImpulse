using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger.Raylib_cs;

using MusicAndTrack = (MusicTrack MusicTrack, Music Music);

public sealed class RaylibAudioService : IAudioService
{
    private MusicAndTrack? _maybeCurrentMusicAndTrack = null;

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
        Sound? maybeSound = soundEffect switch
        {
            SoundEffect.BattleStart => RaylibResources.BattleStartSound,
            SoundEffect.OpenChest => RaylibResources.OpenChestSound,
            SoundEffect.Smack => RaylibResources.SmackSound,
            SoundEffect.Step => RaylibResources.StepSound,
            SoundEffect.Talk => RaylibResources.TalkSound,
            _ => null,
        };

        if (maybeSound is Sound sound)
        {
            PlaySound(sound);
        }
    }

    public void ChangeMusic(MusicTrack musicTrack)
    {
        Music? maybeNewMusic = musicTrack switch
        {
            MusicTrack.Wandering => RaylibResources.Music,
            MusicTrack.Battle => RaylibResources.BattleMusic,
            _ => null,
        };

        if (maybeNewMusic is Music music)
        {
            if (_maybeCurrentMusicAndTrack is MusicAndTrack musicAndTrack)
            {
                StopMusicStream(musicAndTrack.Music);
            }

            _maybeCurrentMusicAndTrack = (MusicAndTrack?)(musicTrack, music);
            PlayMusicStream(music);
        }
    }

    public void Update()
    {
        if (_maybeCurrentMusicAndTrack is MusicAndTrack musicAndTrack)
        {
            UpdateMusicStream(musicAndTrack.Music);
        }
    }
}
