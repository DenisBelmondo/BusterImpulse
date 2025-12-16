using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

public class BattleGoon(GameContext gameContext)
{
    public struct Bullet
    {
        public int HorizontalDirection;
        public float Closeness;
    }

    public struct ShakeContext
    {
        public float Offset;
        public float Interval;
        public float SecondsLeft;
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

    public ShakeContext CurrentShakeContext;
    public float Health = 2;
    public int Animation;
    public float AnimationT;
    public float FlyOffscreenAnimationT;

    static BattleGoon()
    {
        IdleState = new()
        {
            EnterFunction = static self =>
            {
                self.Animation = 0;
                self.AnimationT = 0;
            },
        };

        BeginAttackState = new()
        {
            EnterFunction = static self =>
            {
                self.Animation = 1;
                self.AnimationT = 0;
            },

            UpdateFunction = static self =>
            {
                self.AnimationT += (float)self._gameContext.TimeContext.Delta;

                if (self.AnimationT >= 1)
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
                self.Animation = 2;
                self.AnimationT = 0;
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

                self.AnimationT += (float)self._gameContext.TimeContext.Delta;
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

                if (self.AnimationT >= 5)
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
                self.Animation = 3;
                self.AnimationT = 0;
                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Hough);
            },

            UpdateFunction = static self =>
            {
                self.AnimationT += (float)self._gameContext.TimeContext.Delta;

                if (self.AnimationT >= 1)
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
                self.CurrentShakeContext.Offset = 0.05f;
                self.CurrentShakeContext.Interval = 0;
                self.CurrentShakeContext.SecondsLeft = 0.125f;
            },

            UpdateFunction = static self =>
            {
                self.CurrentShakeContext.Interval += (float)self._gameContext.TimeContext.Delta;
                self.CurrentShakeContext.SecondsLeft -= (float)self._gameContext.TimeContext.Delta;

                if (self.CurrentShakeContext.Interval >= 0.05)
                {
                    self.CurrentShakeContext.Interval = 0;
                    self.CurrentShakeContext.Offset = -self.CurrentShakeContext.Offset;
                }

                if (self.CurrentShakeContext.SecondsLeft <= 0)
                {
                    self.CurrentShakeContext.SecondsLeft = 0;
                    return State<BattleGoon>.Stop;
                }

                return State<BattleGoon>.Continue;
            },

            ExitFunction = static self =>
            {
                self.CurrentShakeContext.Offset = 0;
            },
        };

        DyingState = new()
        {
            EnterFunction = static self =>
            {
                self.Animation = 3;
                self.AnimationT = 0;
            },

            UpdateFunction = static self =>
            {
                self.AnimationT += (float)self._gameContext.TimeContext.Delta;

                if (self.AnimationT >= 1)
                {
                    self.AnimationT = 1;
                    return State<BattleGoon>.Goto(FlyOffscreenState);
                }

                return State<BattleGoon>.Continue;
            },
        };

        FlyOffscreenState = new()
        {
            EnterFunction = static self =>
            {
                self.Animation = 4;
                self.FlyOffscreenAnimationT = 0;
                self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Die);
            },

            UpdateFunction = static self =>
            {
                self.FlyOffscreenAnimationT += (float)self._gameContext.TimeContext.Delta / 1.5f;

                if (self.FlyOffscreenAnimationT >= 1)
                {
                    self.FlyOffscreenAnimationT = 1;
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
    }

    public void Damage(float amount)
    {
        Health -= amount;
        Bullets.Clear();

        StateAutomaton.ChangeState(DamagedState);

        if (Health <= 0)
        {
            StateAutomaton.ChangeState(DyingState);
        }

        ShakeStateAutomaton.ChangeState(ShakeState);
    }
}
