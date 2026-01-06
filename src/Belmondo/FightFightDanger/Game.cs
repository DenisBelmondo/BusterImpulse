namespace Belmondo.FightFightDanger;

using GameStateAutomaton = StateAutomaton<Game, Game.State>;
using PlaysimStateResult = StateAutomaton<Game, Game.State>.Result;
using TransitionStateAutomaton = StateAutomaton<Game.TransitionContext, Game.TransitionContext.State>;

public class Game : IThinker
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

    public class TransitionContext(TimeContext timeContext) : IThinker, IResettable
    {
        public enum State
        {
            Waiting,
            FadingOut,
            StickingAround,
            FadingIn,
        }

        public static event Action<TransitionContext>? FadedOut;

        public Timer Timer = new(timeContext);
        public double FadeT;

        public TransitionStateAutomaton StateAutomaton = new()
        {
            EnterFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                        self.Timer.Start(1.0 / 1.5);
                        break;

                    case State.StickingAround:
                        self.Timer.Start(1.0 / 10.0);
                        break;

                    case State.FadingIn:
                        self.Timer.Start(1.0 / 1.5);
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

                        if (self.Timer.CurrentStatus == Timer.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.StickingAround);
                        }

                        break;

                    case State.StickingAround:
                        if (self.Timer.CurrentStatus == Timer.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.FadingIn);
                        }

                        break;

                    case State.FadingIn:
                        self.FadeT = 1 + self.Timer.GetProgress();

                        if (self.Timer.CurrentStatus == Timer.Status.Stopped)
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
                        self.Reset();
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

        public void Reset()
        {
            FadeT = default;
            Timer.Reset();
        }
    }

    public class MenuContext : IResettable
    {
        public readonly Menu MainMenu = new();
        public readonly Menu SnacksMenu = new();
        public readonly Stack<Menu> MenuStack = [];

        public MenuContext()
        {
            Menus.InitializeMainMenu(MainMenu);
        }

        public void Reset()
        {
            MenuStack.Clear();
        }
    }

    private readonly GameContext _gameContext;

    public event Action? PlayerAteSnack;
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
                    var world = self.World;

                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        var (X, Y) = Direction.ToInt32Tuple(world.Player.Transform.Direction);
                        var desiredX = world.Player.Transform.Position.X + X;
                        var desiredY = world.Player.Transform.Position.Y + Y;

                        GameLogic.TryToInteractWithChest(self._gameContext, world, (desiredX, desiredY));
                    }

                    GameLogic.UpdatePlayer(self._gameContext, world);
                    GameLogic.UpdateChests(self._gameContext, world);
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
                        if (menu.Items.Count > 0)
                        {
                            switch (menu.ID)
                            {
                                case (int)Menus.ID.MainMenu:
                                    var id = (Menus.Item)menu.Items[menu.CurrentItem].ID;

                                    switch (id)
                                    {
                                        case Menus.Item.Snacks:
                                            self.CurrentMenuContext.SnacksMenu.Reset();

                                            if (self.World is not null)
                                            {
                                                Menus.InitializeSnacksMenu(self.CurrentMenuContext.SnacksMenu, self.World.Player.Value.Inventory);
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

                                case (int)Menus.ID.SnacksMenu:
                                    if (self.World is not null)
                                    {
                                        var ateSnack = GameLogic.EatSnack(
                                            ref self.World.Player.Value,
                                            (SnackType)menu.Items[menu.CurrentItem].ID,
                                            self.PlayerAteSnack);

                                        if (ateSnack)
                                        {
                                            var menuItem = menu.Items[menu.CurrentItem];

                                            menuItem.Quantity -= 1;
                                            menu.Items[menu.CurrentItem] = menuItem;
                                        }
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
                if (self.Battle is Battle battle)
                {
                    battle.Update();

                    if (!battle.StateAutomaton.IsRunning())
                    {
                        return PlaysimStateResult.Goto(State.Exploring);
                    }

                    if (battle.CurrentFoe is Foe foe)
                    {
                        foreach (var bullet in foe.Bullets)
                        {
                            if (bullet.Closeness >= 0.9 && bullet.Closeness < 0.95)
                            {
                                var shouldHurtPlayer = bullet.HorizontalDirection == -MathF.Sign(battle.CurrentPlayingContext.PlayerDodgeT);

                                shouldHurtPlayer &= battle.CurrentPlayingContext.PlayerInvulnerabilityT == 0;

                                if (shouldHurtPlayer)
                                {
                                    if (self.World is not null)
                                    {
                                        self.World.Player.Value.Current.Health -= 10;
                                        self.PlayerDamaged?.Invoke();
                                    }

                                    battle.CurrentPlayingContext.PlayerInvulnerabilityT = 1;
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

        PlayerAteSnack += () =>
        {
            _gameContext.AudioService.PlaySoundEffect(SoundEffect.HPUp);
        };

        TransitionContext.FadedOut += transitionContext =>
        {
            if (StateAutomaton.IsProcessingState(State.Transitioning))
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
        CurrentMenuContext.MainMenu.Reset();
        CurrentMenuContext.MenuStack.Clear();
        CurrentMenuContext.MenuStack.Push(CurrentMenuContext.MainMenu);
        StateAutomaton.Push(State.Menu);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.UIConfirm);
    }

    public void CloseAllMenus()
    {
        StateAutomaton.Pop();
        CurrentMenuContext.Reset();
    }

    public void Update()
    {
        if (World is not null)
        {
            var world = World;
            var bleedFreq = (float)_gameContext.TimeContext.Delta * 3;
            var player = world.Player.Value;

            if (player.RunningHealth < player.Current.Health)
            {
                player.RunningHealth += bleedFreq;

                if (player.RunningHealth >= player.Current.Health)
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

            world.Player.Value = player;
        }

        StateAutomaton.Update(this);
        CurrentTransitionContext.Update();
    }
}
