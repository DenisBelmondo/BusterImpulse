namespace Belmondo.FightFightDanger;

public class AnimationContext<TKeyframe>(TimeContext timeContext) : IThinker
{
    public readonly TimerContext TimerContext = new(timeContext);
    public Animation<TKeyframe>? CurrentAnimation;

    public void Play(Animation<TKeyframe> animation)
    {
        CurrentAnimation = animation;
    }

    public void Update()
    {
        TimerContext.Update();
    }
}
