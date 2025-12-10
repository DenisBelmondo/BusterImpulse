namespace Belmondo.FightFightDanger;

public class Game
{
    public event Action? PlayerDamaged;
    public event Action? EnemyDamaged;
    public event Action? EnemyDied;

    public required GameContext? GameContext;

    private bool _shouldTickHealth;
    private double _healthTickDownAccumulator;

    public World? World;

    public BattleState BattleState;
    public DialogState DialogState;

    //
    // temp
    //
    public StateAutomaton StateAutomaton = new();
    public StateAutomaton FoeVisualStateAutomaton = new();
    public StateAutomaton DialogStateAutomaton = new();
    public Log Log = new(8);

    //
    // playsim states
    //

    public State? ExploreState;
    public State? BattleScreenWipe;
    public State? BattleStart;
    public State? BattleStartPlayerAttack;
    public State? BattlePlayerAiming;
    public State? BattlePlayerAttack;
    public State? BattlePlayerMissed;
    public State? BattlePlayerDefend;
    public State? BattlePlayerItem;
    public State? BattlePlayerRun;
    public State? BattleEnemyStartAttack;
    public State? BattleEnemyAttack;
    public State? BattlePlayerDied;

    //
    // dialog states
    //

    public State? DialogSpeakingState;
    public State? DialogFinishedState;

    public void Initialize()
    {
        GameContext gameContext = GameContext!;
        World world = World!;

        ExploreState = new()
        {
            EnterFunction = () =>
            {
                Log.Clear();
                gameContext.AudioService.ChangeMusic(MusicTrack.WanderingStage1);
            },
            UpdateFunction = () =>
            {
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
                    var desiredX = world.Player.Transform.Position.X + X;
                    var desiredY = world.Player.Transform.Position.Y + Y;

                    GameLogic.TryToInteractWithChest(gameContext, ref world, (desiredX, desiredY));
                }

                if (gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    BattleState.Foe = new(FightFightDangerBattleStats.Goon);

                    return State.Goto(BattleScreenWipe!);
                }

                GameLogic.UpdatePlayer(gameContext, ref world);
                GameLogic.UpdateChests(gameContext, ref world);

                return State.Continue;
            }
        };

        BattleScreenWipe = new()
        {
            EnterFunction = () =>
            {
                BattleState.ScreenWipeState.T = 0;
                gameContext.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
            },

            UpdateFunction = () =>
            {
                BattleState.ScreenWipeState.T += (float)gameContext.Delta;

                if (BattleState.ScreenWipeState.T >= 1.5f)
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
                _shouldTickHealth = true;
                gameContext.AudioService.ChangeMusic(MusicTrack.Battle);
                Log.Clear();
                Log.Add("What will you do?");
                Log.Add("A: Attack");
                Log.Add("I: Items");
                Log.Add("D: Defend");
                Log.Add("R: Run");
            },

            UpdateFunction = () =>
            {
                if (gameContext.InputService.ActionWasJustPressed(InputAction.BattleAttack))
                {
                    return State.Goto(BattleStartPlayerAttack!);
                }
                else if (gameContext.InputService.ActionWasJustPressed(InputAction.BattleDefend))
                {
                    return State.Goto(BattlePlayerDefend!);
                }
                else if (gameContext.InputService.ActionWasJustPressed(InputAction.BattleRun))
                {
                    return State.Goto(BattlePlayerRun!);
                }
                else if (gameContext.InputService.ActionWasJustPressed(InputAction.BattleItem))
                {
                    return State.Goto(BattlePlayerItem!);
                }

                if (world.Player.Value.RunningHealth <= 0)
                {
                    return State.Goto(BattlePlayerDied!);
                }

                return State.Continue;
            },
        };

        BattlePlayerItem = new()
        {
            EnterFunction = () =>
            {
                Log.Clear();
                Log.Add("1: Chicken Leg");
                Log.Add("2: Whole Chicken");
                Log.Add("Backspace: Cancel");
            },

            UpdateFunction = () =>
            {
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Cancel))
                {
                    return State.Goto(BattleStart);
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
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
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
                BattleState.PlayerAimingState.CurrentRange = (-0.3, 0.3);
            },
            UpdateFunction = () =>
            {
                BattleState.PlayerAimingState.CurrentAimValue = Math.Sin(gameContext.Time * 4);

                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    if (!BattleState.PlayerAimingState.IsInRange())
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
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
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
                BattleState.Foe.Current.Health -= 1;
                EnemyDamaged?.Invoke();
                Log.Add("Player deals 1 damage!");

                if (BattleState.Foe.Current.Health <= 0)
                {
                    EnemyDied?.Invoke();
                    Log.Add("Enemy defeated.");
                    _shouldTickHealth = false;
                }

                gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
            },
            UpdateFunction = () =>
            {
                if (BattleState.Foe.Current.Health <= 0)
                {
                    if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        return State.Goto(ExploreState);
                    }
                }

                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
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
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
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
                int damage = Random.Shared.Next(0, 20);

                world.Player.Value.Current.Health -= damage;
                gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                PlayerDamaged?.Invoke();
                Log.Add($"\tEnemy deals {damage} damage!");
            },
            UpdateFunction = () =>
            {
                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    return State.Goto(BattleStart);
                }

                return State.Continue;
            }
        };

        BattlePlayerDied = new()
        {
            EnterFunction = () =>
            {
                Log.Clear();
                Log.Add("Oops, you died.");
            },

            UpdateFunction = () =>
            {
                return State.Stop;
            },
        };

        DialogSpeakingState = new()
        {
            UpdateFunction = () =>
            {
                DialogState.CurrentCharacterIndex += (float)gameContext.Delta * 30f;

                if (DialogState.CurrentCharacterIndex >= DialogState.TargetLine?.Length)
                {
                    return State.Goto(DialogFinishedState!);
                }

                if ((int)DialogState.CurrentCharacterIndex >= DialogState.RunningLine.Length)
                {
                    DialogState.RunningLine.Append(DialogState.TargetLine?[(int)DialogState.CurrentCharacterIndex]);
                    gameContext.AudioService.PlaySoundEffect(SoundEffect.Talk);
                }

                if (gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                {
                    DialogState.RunningLine.Clear();
                    DialogState.RunningLine.Append(DialogState.TargetLine);

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
        GameContext gameContext = GameContext!;
        World world = World!;

        if (_shouldTickHealth)
        {
            _healthTickDownAccumulator += gameContext.Delta;

            if (_healthTickDownAccumulator >= 0.5f)
            {
                if (world.Player.Value.Current.Health != world.Player.Value.RunningHealth)
                {
                    world.Player.Value.RunningHealth += MathF.Sign(world.Player.Value.Current.Health - world.Player.Value.RunningHealth);
                }

                _healthTickDownAccumulator = 0;
            }
        }

        StateAutomaton.Update();
        FoeVisualStateAutomaton.Update();
        DialogStateAutomaton.Update();
    }

    public void Say(string line)
    {
        DialogState.TargetLine = line;
        DialogStateAutomaton.CurrentState = DialogSpeakingState;
    }
}
