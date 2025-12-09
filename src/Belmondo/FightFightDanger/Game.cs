namespace Belmondo.FightFightDanger;

public class Game
{
    public GameState GameState = new();

    public World World;

    //
    // temp
    //
    public ShakeStateContext? ShakeStateContext = null;
    public StateAutomaton StateAutomaton = new();
    public StateAutomaton FoeVisualStateAutomaton = new();
    public StateAutomaton DialogStateAutomaton = new();
    public Log Log = new(8);

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
        World = new(services, timeContext);
        ShakeStateContext = new(timeContext);

        ExploreState = new()
        {
            EnterFunction = () =>
            {
                Log.Clear();
                services.AudioService.ChangeMusic(MusicTrack.WanderingStage1);
            },
            UpdateFunction = () =>
            {
                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    var (X, Y) = Direction.ToInt32Tuple(GameState.WorldState.Player.Transform.Direction);
                    var desiredX = GameState.WorldState.Player.Transform.Position.X + X;
                    var desiredY = GameState.WorldState.Player.Transform.Position.Y + Y;

                    World.TryToInteractWithChest(ref GameState.WorldState, (desiredX, desiredY));
                }

                if (services.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    GameState.BattleState.Foe = new(BattleStats.Goon);
                    return State.Goto(BattleScreenWipe!);
                }

                World.UpdatePlayer(ref GameState.WorldState);
                World.UpdateChests(ref GameState.WorldState);

                return State.Continue;
            }
        };

        BattleScreenWipe = new()
        {
            EnterFunction = () =>
            {
                GameState.BattleState.ScreenWipeState.T = 0;
                services.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
            },

            UpdateFunction = () =>
            {
                GameState.BattleState.ScreenWipeState.T += (float)timeContext.Delta;

                if (GameState.BattleState.ScreenWipeState.T >= 1.5f)
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
                GameState.BattleState.PlayerAimingState.CurrentRange = (-0.3, 0.3);
            },
            UpdateFunction = () =>
            {
                GameState.BattleState.PlayerAimingState.CurrentAimValue = Math.Sin(timeContext.Time * 4);

                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    if (!GameState.BattleState.PlayerAimingState.IsInRange())
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
                GameState.BattleState.Foe.Current.Health -= 1;
                Log.Add("Player deals 1 damage!");

                if (GameState.BattleState.Foe.Current.Health <= 0)
                {
                    Log.Add("Enemy defeated.");
                }

                services.AudioService.PlaySoundEffect(SoundEffect.Smack);
                ShakeStateContext?.Shake(0.5f);
            },
            UpdateFunction = () =>
            {
                if (GameState.BattleState.Foe.Current.Health <= 0)
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
                GameState.DialogState.CurrentCharacterIndex += (float)timeContext.Delta * 30f;

                if (GameState.DialogState.CurrentCharacterIndex >= GameState.DialogState.TargetLine?.Length)
                {
                    return State.Goto(DialogFinishedState!);
                }

                if ((int)GameState.DialogState.CurrentCharacterIndex >= GameState.DialogState.RunningLine.Length)
                {
                    GameState.DialogState.RunningLine.Append(GameState.DialogState.TargetLine?[(int)GameState.DialogState.CurrentCharacterIndex]);
                    services.AudioService.PlaySoundEffect(SoundEffect.Talk);
                }

                if (services.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    GameState.DialogState.RunningLine.Clear();
                    GameState.DialogState.RunningLine.Append(GameState.DialogState.TargetLine);

                    return State.Goto(DialogFinishedState!);
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
        GameState.DialogState.TargetLine = line;
        DialogStateAutomaton.CurrentState = DialogSpeakingState;
    }
}
