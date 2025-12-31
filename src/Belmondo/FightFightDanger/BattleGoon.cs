using System.Numerics;
using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

using BattleGoonStateAutomaton = StateAutomaton<BattleGoon, BattleGoon.State>;
using ShakeStateAutomaton = StateAutomaton<BattleGoon, BinaryState>;

public class BattleGoon(GameContext gameContext) : IThinker
{
    public enum State
    {
        Idle,
        BeginAttacking,
        Attacking,
        Hurt,
        Dying,
        FlyingOffscreen,
    }

    public struct Bullet
    {
        public int HorizontalDirection;
        public float Closeness;
    }

    public struct AnimationContext(TimeContext timeContext) : IThinker, IResettable
    {
        public Timer AnimationTimer = new(timeContext);
        public Timer FlyOffscreenTimer = new(timeContext);
        public Timer ShakeTimer = new(timeContext);
        public Vector2 ShakeOffset;
        public int Animation;

        public void Reset()
        {
            AnimationTimer.Reset();
            FlyOffscreenTimer.Reset();
            ShakeOffset = Vector2.Zero;
            Animation = 0;
        }

        public readonly void Update()
        {
            AnimationTimer.Update();
            FlyOffscreenTimer.Update();
            ShakeTimer.Update();
        }
    }

    private readonly GameContext _gameContext = gameContext;
    private double _shootInterval = 0.5;

    public readonly List<Bullet> Bullets = [];

    public readonly BattleGoonStateAutomaton StateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case State.Idle:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 0;

                    break;
                }

                case State.BeginAttacking:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 1;
                    self.CurrentAnimationContext.AnimationTimer.Start(1);

                    break;
                }

                case State.Attacking:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 2;
                    self.CurrentAnimationContext.AnimationTimer.Start(5);

                    break;
                }

                case State.Hurt:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 3;
                    self.CurrentAnimationContext.AnimationTimer.Start(1);
                    self.Bullets.Clear();
                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Hough);

                    break;
                }

                case State.Dying:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 3;
                    self.CurrentAnimationContext.AnimationTimer.Start(1);

                    break;
                }

                case State.FlyingOffscreen:
                {
                    self.CurrentAnimationContext.Reset();
                    self.CurrentAnimationContext.Animation = 4;
                    self.CurrentAnimationContext.FlyOffscreenTimer.Start(2f);
                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Die);

                    break;
                }
            }

            return BattleGoonStateAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case State.BeginAttacking:
                {
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return BattleGoonStateAutomaton.Result.Goto(State.Attacking);
                    }

                    break;
                }

                case State.Attacking:
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

                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return BattleGoonStateAutomaton.Result.Goto(State.Idle);
                    }

                    break;
                }

                case State.Hurt:
                {
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return BattleGoonStateAutomaton.Result.Goto(State.Idle);
                    }

                    break;
                }

                case State.Dying:
                {
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return BattleGoonStateAutomaton.Result.Goto(State.FlyingOffscreen);
                    }

                    break;
                }

                case State.FlyingOffscreen:
                {
                    if (self.CurrentAnimationContext.FlyOffscreenTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return BattleGoonStateAutomaton.Result.Stop;
                    }

                    break;
                }
            }

            return BattleGoonStateAutomaton.Result.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            if (currentState == State.Attacking)
            {
                self.Bullets.Clear();
            }

            return BattleGoonStateAutomaton.Result.Continue;
        },
    };

    public readonly ShakeStateAutomaton ShakeStateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            if (currentState == BinaryState.On)
            {
                self.CurrentAnimationContext.ShakeTimer.Start(0.125f);
            }

            return ShakeStateAutomaton.Result.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            if (currentState == BinaryState.On)
            {
                if (self.CurrentAnimationContext.ShakeTimer.CurrentStatus == Timer.Status.Stopped)
                {
                    return ShakeStateAutomaton.Result.Stop;
                }

                self.CurrentAnimationContext.ShakeOffset = Vector2.UnitX
                    * ((float)Math2.SampleTriangleWave(self._gameContext.TimeContext.Time * 50) / 15f);
            }

            return ShakeStateAutomaton.Result.Continue;
        },
    };

    public AnimationContext CurrentAnimationContext = new(gameContext.TimeContext);

    public float Health = 2;

    public void Update()
    {
        StateAutomaton.Update(this);
        ShakeStateAutomaton.Update(this);
        CurrentAnimationContext.Update();
    }

    public void Damage(float amount)
    {
        Health -= amount;

        StateAutomaton.ChangeState(State.Hurt);

        if (Health <= 0)
        {
            StateAutomaton.ChangeState(State.Dying);
        }

        ShakeStateAutomaton.ChangeState(BinaryState.On);
    }
}
