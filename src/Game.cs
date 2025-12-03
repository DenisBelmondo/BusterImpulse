using System.ComponentModel;
using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo;

public static partial class FightFightDanger
{
    public class Game
    {
        public struct PlayerAimingStateContext
        {
            public (double, double) CurrentRange;
            public double CurrentAimValue;

            public readonly bool IsInRange() => CurrentAimValue >= CurrentRange.Item1 && CurrentAimValue <= CurrentRange.Item2;
        }

        public BattleFoe? CurrentFoe = null;
        public ShakeStateContext ShakeStateContext = new();
        public World? World = null;
        public StateAutomaton StateAutomaton = new();
        public StateAutomaton FoeVisualStateAutomaton = new();
        public Log Log = new(8);
        public RaylibJukebox Jukebox = new();
        public TimeContext TimeContext = new();

        //
        // playsim states
        //
        public State? ExploreState = null;
        public State? BattleStart = null;
        public State? BattleStartPlayerAttack = null;
        public State? BattlePlayerAiming = null;
        public State? BattlePlayerAttack = null;
        public State? BattlePlayerMissed = null;
        public State? BattlePlayerDefend = null;
        public State? BattlePlayerRun = null;
        public State? BattleEnemyStartAttack = null;
        public State? BattleEnemyAttack = null;

        public PlayerAimingStateContext CurrentPlayerAimingStateContext;

        public Game()
        {
            ExploreState = new()
            {
                EnterFunction = () =>
                {
                    Log.Clear();
                    Jukebox.ChangeMusic(Resources.Music);
                },
                UpdateFunction = () =>
                {
                    if (IsKeyPressed(KeyboardKey.B))
                    {
                        CurrentFoe = new(BattleFoe.Stats.Goon);
                        return new(() => BattleStart);
                    }

                    World?.UpdatePlayer(TimeContext.Delta);

                    return State.None;
                }
            };

            BattleStart = new()
            {
                EnterFunction = () =>
                {
                    Jukebox.ChangeMusic(Resources.BattleMusic);
                    Log.Clear();
                    Log.Add("What will you do?");
                    Log.Add("A: Attack");
                    Log.Add("D: Defend");
                    Log.Add("R: Run");
                },
                UpdateFunction = () =>
                {
                    while (true)
                    {
                        var key = GetKeyPressed();

                        if (key == 0)
                        {
                            break;
                        }

                        switch (key)
                        {
                            case KeyboardKey.A:
                                return new(() => BattleStartPlayerAttack);
                            case KeyboardKey.D:
                                return new(() => BattlePlayerDefend);
                            case KeyboardKey.R:
                                return new(() => BattlePlayerRun);
                        }
                    }

                    return State.None;
                },
            };

            BattleStartPlayerAttack = new()
            {
                EnterFunction = () =>
                {
                    Log.Clear();
                    Log.Add("Player attacks!");
                },
                UpdateFunction = () =>
                {
                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        return new(() => BattlePlayerAiming);
                    }

                    return State.None;
                }
            };

            BattlePlayerAiming = new()
            {
                EnterFunction = () =>
                {
                    Log.Clear();
                    Log.Add("Align the circle with the enemy then press enter!");
                    CurrentPlayerAimingStateContext.CurrentRange = (-0.3, 0.3);
                },
                UpdateFunction = () =>
                {
                    CurrentPlayerAimingStateContext.CurrentAimValue = Math.Sin(TimeContext.Time * 4);

                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        if (!CurrentPlayerAimingStateContext.IsInRange())
                        {
                            return new(() => BattlePlayerMissed);
                        }

                        return new(() => BattlePlayerAttack);
                    }

                    return State.None;
                },
            };

            BattlePlayerMissed = new()
            {
                EnterFunction = () =>
                {
                    Log.Clear();
                    Log.Add("Player missed!");
                },
                UpdateFunction = () =>
                {
                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        return new(() => BattleEnemyStartAttack);
                    }

                    return State.None;
                },
            };

            BattlePlayerAttack = new()
            {
                EnterFunction = () =>
                {
                    if (CurrentFoe is not null)
                    {
                        CurrentFoe.Current.Health -= 1;
                        Log.Add("Player deals 1 damage!");

                        if (CurrentFoe.Current.Health <= 0)
                        {
                            Log.Add("Enemy defeated.");
                        }
                    }

                    PlaySound(Resources.SmackSound);
                    ShakeStateContext.Shake(0.5f);
                },
                UpdateFunction = () =>
                {
                    if (CurrentFoe is not null && CurrentFoe.Current.Health <= 0)
                    {
                        if (IsKeyPressed(KeyboardKey.Space))
                        {
                            return new(() => ExploreState);
                        }
                    }

                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        return new(() => BattleEnemyStartAttack);
                    }

                    return State.None;
                },
            };

            BattlePlayerDefend = new()
            {
                EnterFunction = () =>
                {
                    Log.Add("Player defends!");
                },
                UpdateFunction = () =>
                {
                    return new(() => BattleEnemyStartAttack);
                }
            };

            BattlePlayerRun = new()
            {
                EnterFunction = () =>
                {
                    Console.WriteLine("\tPlayer runs!");
                },
                UpdateFunction = () =>
                {
                    return new(() => ExploreState);
                }
            };

            BattleEnemyStartAttack = new()
            {
                EnterFunction = () =>
                {
                    Log.Add("\tEnemy attacks!");
                },
                UpdateFunction = () =>
                {
                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        return new(() => BattleEnemyAttack);
                    }

                    return State.None;
                }
            };

            BattleEnemyAttack = new()
            {
                EnterFunction = () =>
                {
                    Log.Add("\tEnemy deals 1 damage!");
                },
                UpdateFunction = () =>
                {
                    if (IsKeyPressed(KeyboardKey.Space))
                    {
                        return new(() => BattleStart);
                    }

                    return State.None;
                }
            };
        }

        public void Update()
        {
            StateAutomaton.Update();
            FoeVisualStateAutomaton.Update();
            ShakeStateContext.Update(TimeContext.Delta);
        }
    }
}
