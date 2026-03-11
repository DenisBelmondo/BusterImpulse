using System.Numerics;

namespace Belmondo.FightFightDanger;

using FoeStateAutomaton = StateAutomaton<Foe, FoeStateFlag>;
using FoeStateResult = StateFlowResult<FoeStateFlag>;
using ShakeStateAutomaton = StateAutomaton<Foe, BinaryState>;
using ShakeStateResult = StateFlowResult<BinaryState>;

public struct FoeState
{
}

public partial class Foe
{
    public struct Bullet : IRenderable
    {
        public int HorizontalDirection;
        public float Closeness;
        public RenderThing RenderThing;

        public readonly RenderThing GetRenderThing() => RenderThing;
    }

    public struct AnimationContext() : IResettable
    {
        public Timer AnimationTimer = new();
        public Timer FlyOffscreenTimer = new();
        public Timer ShakeTimer = new();
        public Vector2 ShakeOffset;

        public void Reset()
        {
            AnimationTimer.Reset();
            FlyOffscreenTimer.Reset();
            ShakeOffset = Vector2.Zero;
        }

        public readonly void Update(TimeContext timeContext)
        {
            AnimationTimer.Update(timeContext);
            FlyOffscreenTimer.Update(timeContext);
            ShakeTimer.Update(timeContext);
        }
    }

    public static event Action<Foe>? Defeated;

    private readonly GameContext _gameContext;
    public readonly SparseSet<Bullet> Bullets = [];

    public readonly FoeStateAutomaton StateAutomaton = new()
    {
        EnterFunction = static (self, currentState) =>
        {
            switch (self.Type)
            {
                case FoeType.Turret:
                    switch (currentState)
                    {
                        case FoeStateFlag.Idle:
                            self.RenderThing.SubFrame = 0;
                            break;

                        case FoeStateFlag.BeginAttacking:
                            self.RenderThing.SubFrame = 1;
                            self.CurrentAnimationContext.AnimationTimer.Start(1);
                            break;

                        case FoeStateFlag.Attacking:
                            self.RenderThing.SubFrame = 1;
                            self.CurrentAnimationContext.AnimationTimer.Start(5);
                            break;
                    }
                    break;

                case FoeType.Goon:
                    switch (currentState)
                    {
                        case FoeStateFlag.Idle:
                            self.RenderThing.SubFrame = 0;
                            break;

                        case FoeStateFlag.BeginAttacking:
                            self.RenderThing.SubFrame = 0;
                            self.CurrentAnimationContext.AnimationTimer.Start(1);
                            break;

                        case FoeStateFlag.Attacking:
                            self.RenderThing.SubFrame = 1;
                            self.CurrentAnimationContext.AnimationTimer.Start(5);
                            break;

                        case FoeStateFlag.Hurt:
                            self.RenderThing.SubFrame = 2;
                            self.CurrentAnimationContext.AnimationTimer.Start(1);
                            self.Bullets.Clear();
                            self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Hough);
                            break;

                        case FoeStateFlag.Dying:
                            self.RenderThing.SubFrame = 2;
                            self.CurrentAnimationContext.AnimationTimer.Start(1);
                            break;

                        case FoeStateFlag.FlyingOffscreen:
                            self.RenderThing.SubFrame = 3;
                            self.CurrentAnimationContext.FlyOffscreenTimer.Start(2f);
                            self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Die);
                            break;
                    }
                    break;
            }

            return FoeStateResult.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            switch (currentState)
            {
                case FoeStateFlag.BeginAttacking:
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return FoeStateResult.Goto(FoeStateFlag.Attacking);
                    }

                    break;

                case FoeStateFlag.Attacking:
                    switch (self.Type)
                    {
                        case FoeType.Turret:
                            foreach (ref var element in self.Bullets.Span)
                            {
                                Bullet bullet = element.Value;
                                Vector3 bulletOriginalOffset = Vector3.UnitZ * 2;
                                Vector3 bulletOffset = -Vector3.UnitY / 10f;
                                Vector3 bulletDestination = Vector3.Zero;

                                Vector3 bulletPosition = Vector3.Lerp(
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
                                element.Value = bullet;
                            }

                            self._shootInterval += self._gameContext.TimeContext.Delta;

                            if (self._shootInterval >= 1)
                            {
                                self._shootInterval = 0;

                                var horizontalDirection = Random.Shared.Next(-1, 2);

                                self.Bullets.Add(new()
                                {
                                    HorizontalDirection = horizontalDirection,
                                });

                                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Flame);
                            }

                            if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                            {
                                return FoeStateResult.Goto(FoeStateFlag.Idle);
                            }
                            break;

                        case FoeType.Goon:
                            foreach (ref var element in self.Bullets.Span)
                            {
                                Bullet bullet = element.Value;
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
                                element.Value = bullet;
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
                                return FoeStateResult.Goto(FoeStateFlag.Idle);
                            }
                            break;
                    }

                    break;

                case FoeStateFlag.Hurt:
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return FoeStateResult.Goto(FoeStateFlag.Idle);
                    }

                    break;

                case FoeStateFlag.Dying:
                    if (self.CurrentAnimationContext.AnimationTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        return FoeStateResult.Goto(FoeStateFlag.FlyingOffscreen);
                    }

                    break;

                case FoeStateFlag.FlyingOffscreen:
                    if (self.CurrentAnimationContext.FlyOffscreenTimer.CurrentStatus == Timer.Status.Stopped)
                    {
                        Foe.Defeated?.Invoke(self);
                        return FoeStateResult.Stop;
                    }

                    break;
            }

            return FoeStateResult.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            if (currentState == FoeStateFlag.Attacking)
            {
                self.Bullets.Clear();
            }

            return FoeStateResult.Continue;
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

            return ShakeStateResult.Continue;
        },

        UpdateFunction = static (self, currentState) =>
        {
            if (currentState == BinaryState.On)
            {
                if (self.CurrentAnimationContext.ShakeTimer.CurrentStatus == Timer.Status.Stopped)
                {
                    return ShakeStateResult.Stop;
                }

                self.CurrentAnimationContext.ShakeOffset = Vector2.UnitX
                    * ((float)Math2.SampleSquareWave(self._gameContext.TimeContext.Time * 50) / 15f);
            }

            return ShakeStateResult.Continue;
        },

        ExitFunction = static (self, currentState) =>
        {
            if (currentState == BinaryState.On)
            {
                self.CurrentAnimationContext.ShakeOffset = default;
            }

            return ShakeStateResult.Continue;
        },
    };

    public FoeType Type;

    private double _shootInterval = 0.5;

    public AnimationContext CurrentAnimationContext;
    public float Health = 2;

    public Foe(FoeType type, GameContext gameContext)
    {
        Type = type;
        _gameContext = gameContext;
        CurrentAnimationContext = new();
        InitializeRenderThing();
    }

    static Foe()
    {
        InitializeFlyOffscreenControlPoints();
    }

    public void Damage(float amount)
    {
        Health -= amount;
        StateAutomaton.ChangeState(FoeStateFlag.Hurt);

        if (Health <= 0)
        {
            StateAutomaton.ChangeState(FoeStateFlag.Dying);
        }

        ShakeStateAutomaton.ChangeState(BinaryState.On);
    }
}

public partial class Foe
{
    public void Update(TimeContext timeContext)
    {
        StateAutomaton.Update(this);
        ShakeStateAutomaton.Update(this);
        CurrentAnimationContext.Update(timeContext);
        UpdateRenderThing();
    }
}

public partial class Foe : IRenderable
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
