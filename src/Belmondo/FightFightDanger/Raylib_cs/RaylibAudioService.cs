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
            SoundEffect.Item => RaylibResources.ItemSound,
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
            MusicTrack.WanderingStage1 => RaylibResources.Stage1WanderingMusic,
            MusicTrack.WanderingStage2 => RaylibResources.Stage2WanderingMusic,
            MusicTrack.Battle => RaylibResources.BattleMusic,
            _ => null,
        };

        if (maybeNewMusic is Music music)
        {
            if (_maybeCurrentMusicAndTrack is MusicAndTrack musicAndTrack)
            {
                unsafe
                {
                    if (music.Stream == musicAndTrack.Music.Stream)
                    {
                        return;
                    }

                    StopMusicStream(musicAndTrack.Music);
                }
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
