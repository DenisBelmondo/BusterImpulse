namespace Belmondo.FightFightDanger;

using GameStateAutomaton = StateAutomaton<Game, Game.State>;
using PlaysimStateResult = StateAutomaton<Game, Game.State>.Result;
using TransitionStateAutomaton = StateAutomaton<Game.TransitionContext, Game.TransitionContext.State>;

public class Game
{
    public enum State
    {
        Exploring,
        Transitioning,
        Battling,
        Menu,
    }

    public enum RenderHint
    {
        Exploring,
        Battling,
    }

    public class TransitionContext(TimeContext timeContext) : IThinker
    {
        public enum State
        {
            Waiting,
            FadingOut,
            StickingAround,
            FadingIn,
        }

        public static event Action<TransitionContext>? FadedOut;

        public TimerContext Timer = new(timeContext);
        public double FadeT;

        public TransitionStateAutomaton StateAutomaton = new()
        {
            EnterFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                        self.Timer.Start(1);
                        break;

                    case State.StickingAround:
                        self.Timer.Start(0.5);
                        break;

                    case State.FadingIn:
                        self.Timer.Start(1);
                        break;
                }

                return TransitionStateAutomaton.Result.Continue;
            },

            UpdateFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                        self.FadeT = self.Timer.GetProgress();

                        if (self.Timer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.StickingAround);
                        }

                        break;

                    case State.StickingAround:
                        if (self.Timer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.FadingIn);
                        }

                        break;

                    case State.FadingIn:
                        self.FadeT = 1 + self.Timer.GetProgress();

                        if (self.Timer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Stop;
                        }

                        break;
                }

                return TransitionStateAutomaton.Result.Continue;
            },

            ExitFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                        FadedOut?.Invoke(self);
                        break;

                    case State.FadingIn:
                        self.Timer.Reset();
                        break;
                }

                return TransitionStateAutomaton.Result.Continue;
            },
        };

        public void Update()
        {
            StateAutomaton.Update(this);
            Timer.Update();
        }
    }

    public class MenuContext : IResettable
    {
        public Menu SnacksMenu = new();

        public Stack<Menu> MenuStack = [];

        public void Reset()
        {
            MenuStack.Clear();
        }
    }

    private readonly GameContext _gameContext;

    public event Action? PlayerDamaged;
    public event Action? Quit;

    public World? World;
    public Battle Battle;

    public readonly GameStateAutomaton StateAutomaton = new()
    {
        EnterFunction = EnterFunction,
        UpdateFunction = UpdateFunction,
    };

    public readonly Log Log = new(8);

    public RenderHint CurrentRenderHint;
    public TransitionContext CurrentTransitionContext;
    public MenuContext CurrentMenuContext;

    private static PlaysimStateResult EnterFunction(Game self, State currentState)
    {
        switch (currentState)
        {
            case State.Exploring:
                self.Log.Clear();
                self.CurrentRenderHint = RenderHint.Exploring;
                self._gameContext.AudioService.ChangeMusic(MusicTrack.WanderingStage1);

                break;

            case State.Transitioning:
                self.CurrentTransitionContext.StateAutomaton.ChangeState(TransitionContext.State.FadingOut);
                break;

            case State.Battling:
                self.CurrentRenderHint = RenderHint.Battling;
                self._gameContext.AudioService.ChangeMusic(MusicTrack.Battle);
                break;
        }

        return PlaysimStateResult.Continue;
    }

    private static PlaysimStateResult UpdateFunction(Game self, State currentState)
    {
        switch (currentState)
        {
            case State.Exploring:
                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    self.StartBattle();
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Cancel))
                {
                    self.OpenMainMenu();
                }

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

                break;

            case State.Menu:
                var audio = self._gameContext.AudioService;
                var input = self._gameContext.InputService;

                if (self.CurrentMenuContext.MenuStack.TryPeek(out Menu? menu))
                {
                    if (input.ActionWasJustPressed(InputAction.MoveBack))
                    {
                        menu.CurrentItem += 1;

                        if (menu.CurrentItem >= menu.Items.Count)
                        {
                            menu.CurrentItem = 0;
                        }

                        audio.PlaySoundEffect(SoundEffect.UIFocus);
                    }
                    else if (input.ActionWasJustPressed(InputAction.MoveForward))
                    {
                        menu.CurrentItem -= 1;

                        if (menu.CurrentItem < 0)
                        {
                            menu.CurrentItem = menu.Items.Count - 1;
                        }

                        audio.PlaySoundEffect(SoundEffect.UIFocus);
                    }
                    else if (input.ActionWasJustPressed(InputAction.Confirm))
                    {
                        switch (menu.ID)
                        {
                            case (int)Menus.ID.MainMenu:
                                {
                                    var id = (Menus.Item)menu.Items[menu.CurrentItem].ID;

                                    switch (id)
                                    {
                                        case Menus.Item.Snacks:
                                            self.CurrentMenuContext.SnacksMenu.Reset();

                                            if (self.World is World world)
                                            {
                                                Menus.InitializeSnacksMenu(self.CurrentMenuContext.SnacksMenu, world.Player.Value.Inventory);
                                            }

                                            self.CurrentMenuContext.MenuStack.Push(self.CurrentMenuContext.SnacksMenu);

                                            break;

                                        case Menus.Item.Charms:
                                            Console.WriteLine("Charms");
                                            break;

                                        case Menus.Item.Quit:
                                            self.Quit?.Invoke();
                                            break;
                                    }

                                    break;
                                }
                        }

                        audio.PlaySoundEffect(SoundEffect.UIConfirm);
                    }
                }

                if (input.ActionWasJustPressed(InputAction.Cancel))
                {
                    self.CurrentMenuContext.MenuStack.TryPop(out var _);

                    if (self.CurrentMenuContext.MenuStack.Count == 0)
                    {
                        self.CloseAllMenus();
                    }

                    audio.PlaySoundEffect(SoundEffect.UICancel);
                }

                break;

            case State.Transitioning:
                if (!self.CurrentTransitionContext.StateAutomaton.IsRunning())
                {
                    return PlaysimStateResult.Goto(State.Battling);
                }

                break;

            case State.Battling:
                if (self.World is not null)
                {
                    var bleedFreq = (float)self._gameContext.TimeContext.Delta * 3;
                    var player = self.World.Player.Value;

                    if (player.RunningHealth < player.Current.Health)
                    {
                        player.RunningHealth += bleedFreq;

                        if (player.RunningHealth <= player.Current.Health)
                        {
                            player.RunningHealth = player.Current.Health;
                        }
                    }
                    else if (player.RunningHealth > player.Current.Health)
                    {
                        player.RunningHealth -= bleedFreq;

                        if (player.RunningHealth <= player.Current.Health)
                        {
                            player.RunningHealth = player.Current.Health;
                        }
                    }

                    self.World.Player.Value = player;
                }

                if (self.Battle is not null)
                {
                    self.Battle.Update();

                    if (self.Battle.StateAutomaton.CurrentState is null)
                    {
                        return PlaysimStateResult.Goto(State.Exploring);
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
                                        self.PlayerDamaged?.Invoke();
                                    }

                                    self.Battle.CurrentPlayingContext.PlayerInvulnerabilityT = 1;
                                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                                }
                            }
                        }
                    }
                }

                break;
        }

        return PlaysimStateResult.Continue;
    }

    public Game(GameContext gameContext)
    {
        _gameContext = gameContext;
        Battle = new(gameContext);
        CurrentTransitionContext = new(gameContext.TimeContext);
        CurrentMenuContext = new();

        TransitionContext.FadedOut += transitionContext =>
        {
            if (StateAutomaton.CurrentState == State.Transitioning)
            {
                CurrentRenderHint = RenderHint.Battling;
            }
            else
            {
                CurrentRenderHint = RenderHint.Exploring;
            }
        };
    }

    public void SetWorld(World world)
    {
        World = world;
    }

    public void StartBattle()
    {
        Battle?.Reset();
        StateAutomaton.ChangeState(State.Transitioning);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
    }

    public void OpenMainMenu()
    {
        CurrentMenuContext.MenuStack.Clear();
        CurrentMenuContext.MenuStack.Push(Menus.Main);
        StateAutomaton.ChangeState(State.Menu);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.UIConfirm);
    }

    public void CloseAllMenus()
    {
        StateAutomaton.ChangeState(State.Exploring);
        CurrentMenuContext.Reset();
    }

    public void Update()
    {
        StateAutomaton.Update(this);
        CurrentTransitionContext.Update();
    }
}
