using System.Text;

namespace Belmondo.FightFightDanger;

public class Game
{
    public struct BattleScreenWipeContext
    {
        public float T;
    }

    public struct PlayerAimingStateContext
    {
        public (double, double) CurrentRange;
        public double CurrentAimValue;

        public readonly bool IsInRange() => CurrentAimValue >= CurrentRange.Item1 && CurrentAimValue <= CurrentRange.Item2;
    }

    public class DialogStateContext(string targetLine)
    {
        public string TargetLine = targetLine;
        public StringBuilder RunningLine = new();
        public float CurrentCharacterIndex;
    }

    //
    // battle stats
    //

    public static readonly BattleFoe.Stats GoonBattleStats = new()
    {
        Health = 5,
        Damage = 1,
    };

    public World? World = null;
    public BattleFoe? CurrentFoe = null;
    public ShakeStateContext? ShakeStateContext = null;
    public StateAutomaton StateAutomaton = new();
    public StateAutomaton FoeVisualStateAutomaton = new();
    public StateAutomaton DialogStateAutomaton = new();
    public Log Log = new(8);

    //
    // state contexts
    //

    public BattleScreenWipeContext CurrentScreenWipeContext;
    public PlayerAimingStateContext CurrentPlayerAimingStateContext;
    public DialogStateContext? CurrentDialogStateContext = null;

    //
    // playsim states
    //

    public State ExploreState;
    public State BattleScreenWipe;
    public State BattleStart;
    public State BattleStartPlayerAttack;
    public State BattlePlayerAiming;
    public State BattlePlayerAttack;
    public State BattlePlayerMissed;
    public State BattlePlayerDefend;
    public State BattlePlayerRun;
    public State BattleEnemyStartAttack;
    public State BattleEnemyAttack;

    //
    // dialog states
    //

    public State DialogSpeakingState;
    public State DialogFinishedState;

    public Game(Services services, TimeContext timeContext)
    {
        ShakeStateContext = new(timeContext);

        ExploreState = new()
        {
            EnterFunction = () =>
            {
                Log.Clear();
                services.AudioService.ChangeMusic(MusicTrack.Wandering);
            },
            UpdateFunction = () =>
            {
                if (World is not null)
                {
                    if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        var (X, Y) = Direction.ToInt32Tuple(World.Player.Transform.Direction);
                        var desiredX = World.Player.Transform.X + X;
                        var desiredY = World.Player.Transform.Y + Y;

                        World.TryToInteractWithChest((desiredX, desiredY));
                    }
                }

                if (services.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    CurrentFoe = new(GoonBattleStats);
                    return State.Goto(BattleScreenWipe!);
                }

                World?.UpdatePlayer();
                World?.UpdateChests();

                return State.Continue;
            }
        };

        BattleScreenWipe = new()
        {
            EnterFunction = () =>
            {
                CurrentScreenWipeContext.T = 0;
                services.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
            },

            UpdateFunction = () =>
            {
                CurrentScreenWipeContext.T += (float)timeContext.Delta;

                if (CurrentScreenWipeContext.T >= 1.5f)
                {
                    return State.Goto(BattleStart!);
                }

                return State.Continue;
            },
        };

        BattleStart = new()
        {
            EnterFunction = () =>
            {
                services.AudioService.ChangeMusic(MusicTrack.Battle);
                Log.Clear();
                Log.Add("What will you do?");
                Log.Add("A: Attack");
                Log.Add("D: Defend");
                Log.Add("R: Run");
            },
            UpdateFunction = () =>
            {
                if (services.InputService.ActionWasJustPressed(InputAction.BattleAttack))
                {
                    return State.Goto(BattleStartPlayerAttack!);
                }
                else if (services.InputService.ActionWasJustPressed(InputAction.BattleDefend))
                {
                    return State.Goto(BattlePlayerDefend!);
                }
                else if (services.InputService.ActionWasJustPressed(InputAction.BattleRun))
                {
                    return State.Goto(BattlePlayerRun!);
                }

                return State.Continue;
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
                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattlePlayerAiming!);
                }

                return State.Continue;
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
                CurrentPlayerAimingStateContext.CurrentAimValue = Math.Sin(timeContext.Time * 4);

                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    if (!CurrentPlayerAimingStateContext.IsInRange())
                    {
                        return State.Goto(BattlePlayerMissed!);
                    }

                    return State.Goto(BattlePlayerAttack!);
                }

                return State.Continue;
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
                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattleEnemyStartAttack!);
                }

                return State.Continue;
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

                services.AudioService.PlaySoundEffect(SoundEffect.Smack);
                ShakeStateContext?.Shake(0.5f);
            },
            UpdateFunction = () =>
            {
                if (CurrentFoe is not null && CurrentFoe.Current.Health <= 0)
                {
                    if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        return State.Goto(ExploreState);
                    }
                }

                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattleEnemyStartAttack!);
                }

                return State.Continue;
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
                return State.Goto(BattleEnemyStartAttack!);
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
                return State.Goto(ExploreState);
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
                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattleEnemyAttack!);
                }

                return State.Continue;
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
                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattleStart);
                }

                return State.Continue;
            }
        };

        DialogSpeakingState = new()
        {
            UpdateFunction = () =>
            {
                if (CurrentDialogStateContext is not null)
                {
                    CurrentDialogStateContext.CurrentCharacterIndex += (float)timeContext.Delta * 30f;

                    if (CurrentDialogStateContext.CurrentCharacterIndex >= CurrentDialogStateContext.TargetLine.Length)
                    {
                        return State.Goto(DialogFinishedState!);
                    }

                    if ((int)CurrentDialogStateContext.CurrentCharacterIndex >= CurrentDialogStateContext.RunningLine.Length)
                    {
                        CurrentDialogStateContext.RunningLine.Append(CurrentDialogStateContext.TargetLine[(int)CurrentDialogStateContext.CurrentCharacterIndex]);
                        services.AudioService.PlaySoundEffect(SoundEffect.Talk);
                    }

                    if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        CurrentDialogStateContext.RunningLine.Clear();
                        CurrentDialogStateContext.RunningLine.Append(CurrentDialogStateContext.TargetLine);

                        return State.Goto(DialogFinishedState!);
                    }
                }

                return State.Continue;
            }
        };

        DialogFinishedState = new()
        {
            UpdateFunction = () =>
            {
                return State.Continue;
            }
        };
    }

    public void Update()
    {
        StateAutomaton.Update();
        FoeVisualStateAutomaton.Update();
        DialogStateAutomaton.Update();
        ShakeStateContext?.Update();
    }

    public void Say(string line)
    {
        CurrentDialogStateContext = new(line);
        DialogStateAutomaton.CurrentState = DialogSpeakingState;
    }
}
