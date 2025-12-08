using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger.Raylib_cs;

public sealed class RaylibAudioService : IAudioService
{
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
}
