namespace Belmondo.FightFightDanger;

public class Game(GameContext gameContext)
{
    //
    // playsim states
    //

    public static State<Game> ExploreState = State<Game>.Empty;
    public static State<Game> BattleState = State<Game>.Empty;

    //
    // instance vars
    //

    private readonly GameContext _gameContext = gameContext;

    public event Action? PlayerDamaged;
    public event Action? EnemyDamaged;
    public event Action? EnemyDied;

    public World? World;
    public Battle Battle = new(gameContext);

    //
    // temp
    //
    public StateAutomaton<Game> StateAutomaton = new();
    public Log Log = new(8);

    static Game()
    {
        ExploreState = new()
        {
            EnterFunction = static self =>
            {
                self.Log.Clear();
                self._gameContext.AudioService.ChangeMusic(MusicTrack.WanderingStage1);
            },

            UpdateFunction = static self =>
            {
                if (self.World is not null)
                {
                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        var (X, Y) = Direction.ToInt32Tuple(self.World.Player.Transform.Direction);
                        var desiredX = self.World.Player.Transform.Position.X + X;
                        var desiredY = self.World.Player.Transform.Position.Y + Y;

                        GameLogic.TryToInteractWithChest(self._gameContext, ref self.World, (desiredX, desiredY));
                    }

                    GameLogic.UpdatePlayer(self._gameContext, ref self.World);
                    GameLogic.UpdateChests(self._gameContext, ref self.World);
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    self.Battle?.Reset();
                    return State<Game>.Goto(BattleState);
                }

                return State<Game>.Continue;
            }
        };

        BattleState = new()
        {
            EnterFunction = static self =>
            {
                self._gameContext.AudioService.ChangeMusic(MusicTrack.Battle);
            },

            UpdateFunction = static self =>
            {
                if (self.World is not null)
                {
                    self.World.Player.Value.RunningHealth += MathF.Sign(self.World.Player.Value.Current.Health - self.World.Player.Value.RunningHealth) * ((float)self._gameContext.TimeContext.Delta * 3);
                }

                if (self.Battle is not null)
                {
                    self.Battle.Update();

                    if (self.Battle.StateAutomaton.CurrentState == State<Battle>.Empty)
                    {
                        return State<Game>.Goto(ExploreState);
                    }

                    if (self.Battle.CurrentBattleGoon is not null)
                    {
                        foreach (var bullet in self.Battle.CurrentBattleGoon.Bullets)
                        {
                            if (bullet.Closeness >= 0.9 && bullet.Closeness < 0.95)
                            {
                                var shouldHurtPlayer = bullet.HorizontalDirection == -MathF.Sign(self.Battle.CurrentPlayingContext.PlayerDodgeT);

                                shouldHurtPlayer &= self.Battle.CurrentPlayingContext.PlayerInvulnerabilityT == 0;

                                if (shouldHurtPlayer)
                                {
                                    if (self.World is not null)
                                    {
                                        self.World.Player.Value.Current.Health -= 10;
                                    }

                                    self.Battle.CurrentPlayingContext.PlayerInvulnerabilityT = 1;
                                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                                }
                            }
                        }
                    }
                }

                return State<Game>.Continue;
            },
        };
    }

    public void Update()
    {
        StateAutomaton.Update(this);
    }
}
