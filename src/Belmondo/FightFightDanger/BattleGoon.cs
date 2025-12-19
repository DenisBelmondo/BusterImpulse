using System.Numerics;
using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

public class BattleGoon(GameContext gameContext) : IThinker
{
    public struct Bullet
    {
        public int HorizontalDirection;
        public float Closeness;
    }

    public struct AnimationContext(TimeContext timeContext) : IThinker, IResettable
    {
        public TimerContext AnimationTimerContext = new(timeContext);
        public TimerContext FlyOffscreenTimerContext = new(timeContext);
        public TimerContext ShakeTimerContext = new(timeContext);
        public Vector2 ShakeOffset;
        public int Animation;

        public void Reset()
        {
            AnimationTimerContext.Reset();
            FlyOffscreenTimerContext.Reset();
            ShakeOffset = Vector2.Zero;
            Animation = 0;
        }

        public readonly void Update()
        {
            AnimationTimerContext.Update();
            FlyOffscreenTimerContext.Update();
            ShakeTimerContext.Update();
        }
    }

    public static State<BattleGoon> IdleState = State<BattleGoon>.Empty;
    public static State<BattleGoon> BeginAttackState = State<BattleGoon>.Empty;
    public static State<BattleGoon> AttackState = State<BattleGoon>.Empty;
    public static State<BattleGoon> DamagedState = State<BattleGoon>.Empty;
    public static State<BattleGoon> DyingState = State<BattleGoon>.Empty;
    public static State<BattleGoon> FlyOffscreenState = State<BattleGoon>.Empty;

    public static State<BattleGoon> ShakeState = State<BattleGoon>.Empty;

    private readonly GameContext _gameContext = gameContext;
    private double _shootInterval = 0.5;

    public readonly List<Bullet> Bullets = [];
    public readonly StateAutomaton<BattleGoon> StateAutomaton = new();
    public readonly StateAutomaton<BattleGoon> ShakeStateAutomaton = new();

    public AnimationContext CurrentAnimationContext = new(gameContext.TimeContext);

    public float Health = 2;

    static BattleGoon()
    {
        IdleState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 0;
            },
        };

        BeginAttackState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 1;
                self.CurrentAnimationContext.AnimationTimerContext.Start(1);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentAnimationContext.AnimationTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Goto(AttackState);
                }

                return State<BattleGoon>.Continue;
            },
        };

        AttackState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 2;
                self.CurrentAnimationContext.AnimationTimerContext.Start(5);
            },

            UpdateFunction = static self =>
            {
                foreach (ref var bullet in CollectionsMarshal.AsSpan(self.Bullets))
                {
                    if (bullet.Closeness >= 1)
                    {
                        bullet.Closeness = 1;
                        continue;
                    }

                    bullet.Closeness += (float)self._gameContext.TimeContext.Delta;
                }

                self._shootInterval += self._gameContext.TimeContext.Delta;

                if (self._shootInterval >= 0.4)
                {
                    self._shootInterval = 0;

                    var horizontalDirection = Random.Shared.Next(-1, 2);

                    self.Bullets.Add(new()
                    {
                        HorizontalDirection = horizontalDirection,
                    });

                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.MachineGun);
                }

                if (self.CurrentAnimationContext.AnimationTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Goto(IdleState);
                }

                return State<BattleGoon>.Continue;
            },

            ExitFunction = static self =>
            {
                self.Bullets.Clear();
            },
        };

        DamagedState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 3;
                self.CurrentAnimationContext.AnimationTimerContext.Start(1);
                self.Bullets.Clear();
                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Hough);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentAnimationContext.AnimationTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Goto(IdleState);
                }

                return State<BattleGoon>.Continue;
            },
        };

        ShakeState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.ShakeTimerContext.Start(0.125f);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentAnimationContext.ShakeTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Stop;
                }

                self.CurrentAnimationContext.ShakeOffset = Vector2.UnitX
                    * ((float)Math2.SampleTriangleWave(self._gameContext.TimeContext.Time * 50) / 15f);

                return State<BattleGoon>.Continue;
            },
        };

        DyingState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 3;
                self.CurrentAnimationContext.AnimationTimerContext.Start(1);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentAnimationContext.AnimationTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Goto(FlyOffscreenState);
                }

                return State<BattleGoon>.Continue;
            },
        };

        FlyOffscreenState = new()
        {
            EnterFunction = static self =>
            {
                self.CurrentAnimationContext.Reset();
                self.CurrentAnimationContext.Animation = 4;
                self.CurrentAnimationContext.FlyOffscreenTimerContext.Start(2f);
                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Die);
            },

            UpdateFunction = static self =>
            {
                if (self.CurrentAnimationContext.FlyOffscreenTimerContext.CurrentStatus == TimerContext.Status.Stopped)
                {
                    return State<BattleGoon>.Stop;
                }

                return State<BattleGoon>.Continue;
            },
        };
    }

    public void Update()
    {
        StateAutomaton.Update(this);
        ShakeStateAutomaton.Update(this);
        CurrentAnimationContext.Update();
    }

    public void Damage(float amount)
    {
        Health -= amount;

        StateAutomaton.ChangeState(DamagedState);

        if (Health <= 0)
        {
            StateAutomaton.ChangeState(DyingState);
        }

        ShakeStateAutomaton.ChangeState(ShakeState);
    }
}
