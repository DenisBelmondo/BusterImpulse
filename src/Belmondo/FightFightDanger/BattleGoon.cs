using System.Numerics;
using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

using BattleGoonStateAutomaton = StateAutomaton<BattleGoon, BattleGoon.State>;
using ShakeStateAutomaton = StateAutomaton<BattleGoon, BinaryState>;

public partial class BattleGoon
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

    public struct Bullet : IRenderable
    {
        public int HorizontalDirection;
        public float Closeness;
        public RenderThing RenderThing;

        public readonly RenderThing GetRenderThing() => RenderThing;
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

    public event Action? Defeated;

    private readonly GameContext _gameContext;
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
                        var bulletOriginalOffset = Vector3.UnitZ * 2;
                        var bulletOffset = -Vector3.UnitY / 10f;
                        var bulletDestination = Vector3.Zero + Vector3.UnitX * bullet.HorizontalDirection / 10f;

                        var bulletPosition = Vector3.Lerp(
                            bulletOriginalOffset + bulletOffset,
                            bulletDestination + bulletOffset,
                            bullet.Closeness);

                        bullet.RenderThing.Transform.Translation = bulletPosition;

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
                        self.Defeated?.Invoke();
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

    private double _shootInterval = 0.5;

    public AnimationContext CurrentAnimationContext;
    public float Health = 2;

    public BattleGoon(GameContext gameContext)
    {
        _gameContext = gameContext;
        CurrentAnimationContext = new(gameContext.TimeContext);
        InitializeRenderThing();
    }

    static BattleGoon()
    {
        InitializeFlyOffscreenControlPoints();
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

public partial class BattleGoon : IThinker
{
    public void Update()
    {
        StateAutomaton.Update(this);
        ShakeStateAutomaton.Update(this);
        CurrentAnimationContext.Update();
        UpdateRenderThing();
    }
}

public partial class BattleGoon : IRenderable
{
    private static Vector3[] _flyOffscreenControlPoints = [];
    public Vector3 BaseOffset;
    public Vector3 FlyOffscreenOffset;
    public RenderThing RenderThing;

    private static void InitializeFlyOffscreenControlPoints()
    {
        _flyOffscreenControlPoints = [
            Vector3.Zero,
            -Vector3.UnitX / 2 + Vector3.UnitY + Vector3.UnitZ,
            -Vector3.UnitX - Vector3.UnitY * 4 + Vector3.UnitZ * 2
        ];
    }

    public void InitializeRenderThing()
    {
        BaseOffset = Vector3.UnitZ * 2;
        UpdateRenderThing();
    }

    public void UpdateRenderThing()
    {
        float t = (float)CurrentAnimationContext.FlyOffscreenTimer.GetProgress();

        FlyOffscreenOffset = Math2.SampleCatmullRom(
            _flyOffscreenControlPoints,
            t);

        RenderThing.Transform.Translation = (
            BaseOffset
                + FlyOffscreenOffset);
    }

    public RenderThing GetRenderThing()
    {
        return RenderThing;
    }
}
