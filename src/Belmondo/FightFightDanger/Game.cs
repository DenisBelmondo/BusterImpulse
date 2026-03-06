using static Belmondo.FightFightDanger.Items;

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

    public class TransitionContext : IThinker<TimeContext>, IResettable
    {
        public enum State
        {
            Waiting,
            FadingOut,
            StickingAround,
            FadingIn,
        }

        public static event Action<TransitionContext>? FadedOut;

        public Timer Timer = new();
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

        public void Update(TimeContext timeContext)
        {
            StateAutomaton.Update(this);
            Timer.Update(timeContext);
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

    public World? CurrentWorld;
    public Battle Battle;
    public Exploration Exploration;

    public readonly GameStateAutomaton StateAutomaton = new()
    {
        EnterFunction = EnterFunction,
        UpdateFunction = UpdateFunction,
    };

    public readonly Log Log = new(8);
    public readonly TransitionContext CurrentTransitionContext;
    public readonly MenuContext CurrentMenuContext;

    public RenderHint CurrentRenderHint;

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
                if (self.CurrentWorld is not null)
                {
                    self.Exploration.Update(self._gameContext, self.CurrentWorld);
                }

                self.BleedOutPlayer();
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

                                            if (self.CurrentWorld is not null)
                                            {
                                                Menus.InitializeSnacksMenu(self.CurrentMenuContext.SnacksMenu, self.CurrentWorld.Player.Value.Inventory);
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
                                    if (self.CurrentWorld is not null)
                                    {
                                        var ateSnack = EatSnack(
                                            ref self.CurrentWorld.Player.Value,
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
                        foreach (var element in foe.Bullets)
                        {
                            Foe.Bullet bullet = element.Value;

                            if (bullet.Closeness >= 0.9 && bullet.Closeness < 0.95)
                            {
                                var shouldHurtPlayer = bullet.HorizontalDirection == -MathF.Sign(battle.CurrentPlayingContext.PlayerDodgeT);

                                shouldHurtPlayer &= battle.CurrentPlayingContext.PlayerInvulnerabilityT == 0;

                                if (shouldHurtPlayer)
                                {
                                    if (self.CurrentWorld is not null)
                                    {
                                        self.CurrentWorld.Player.Value.Current.Health -= 10;
                                        self.PlayerDamaged?.Invoke();
                                    }

                                    battle.CurrentPlayingContext.PlayerInvulnerabilityT = 1;
                                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.Smack);
                                }
                            }
                        }
                    }

                    var shouldBleedOutPlayer = (
                        battle.StateAutomaton.IsProcessingState(Battle.State.Choosing)
                        || battle.StateAutomaton.IsProcessingState(Battle.State.Playing));

                    if (shouldBleedOutPlayer)
                    {
                        self.BleedOutPlayer();
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
        Exploration = new();
        CurrentTransitionContext = new();
        CurrentMenuContext = new();

        Exploration.BattleRequested += StartBattle;
        Exploration.MainMenuRequested += OpenMainMenu;

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
        CurrentWorld = world;
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
        StateAutomaton.Update(this);
        CurrentTransitionContext.Update(_gameContext.TimeContext);
    }

    public void UpdatePlayer()
    {
        if (CurrentWorld is null)
        {
            return;
        }

        var input = _gameContext.InputService;

        if (input.ActionWasJustPressed(InputAction.LookRight))
        {
            CurrentWorld.OldPlayerDirection = CurrentWorld.Player.Transform.Direction;
            CurrentWorld.CameraDirectionLerpT = 0;
            CurrentWorld.Player.Transform.Direction++;
        }
        else if (input.ActionWasJustPressed(InputAction.LookLeft))
        {
            CurrentWorld.OldPlayerDirection = CurrentWorld.Player.Transform.Direction;
            CurrentWorld.CameraDirectionLerpT = 0;
            CurrentWorld.Player.Transform.Direction--;
        }

        CurrentWorld.Player.Transform.Direction = Direction.Clamped(CurrentWorld.Player.Transform.Direction);
        CurrentWorld.CameraDirectionLerpT = MathF.Min(CurrentWorld.CameraDirectionLerpT + (float)_gameContext.TimeContext.Delta, 1);

        int? moveDirection = null;

        if (input.ActionIsPressed(InputAction.MoveForward))
        {
            moveDirection = Direction.Clamped(CurrentWorld.Player.Transform.Direction);
        }
        else if (input.ActionIsPressed(InputAction.MoveBack))
        {
            moveDirection = Direction.Clamped(CurrentWorld.Player.Transform.Direction + 2);
        }
        else if (input.ActionIsPressed(InputAction.MoveLeft))
        {
            moveDirection = Direction.Clamped(CurrentWorld.Player.Transform.Direction + 3);
        }
        else if (input.ActionIsPressed(InputAction.MoveRight))
        {
            moveDirection = Direction.Clamped(CurrentWorld.Player.Transform.Direction + 1);
        }

        if (moveDirection is not null && CurrentWorld.Player.Value.Current.WalkCooldown == 0)
        {
            CurrentWorld.CameraPositionLerpT = 0;
            CurrentWorld.OldPlayerX = CurrentWorld.Player.Transform.Position.X;
            CurrentWorld.OldPlayerY = CurrentWorld.Player.Transform.Position.Y;
            _gameContext.AudioService.PlaySoundEffect(SoundEffect.Step);
            CurrentWorld.Player.Value.Current.WalkCooldown = CurrentWorld.Player.Value.Default.WalkCooldown;
            CurrentWorld.TraceMove(CurrentWorld.Player.Transform.Position, (int)moveDirection, out CurrentWorld.Player.Transform.Position);
        }

        CurrentWorld.Player.Value.Current.WalkCooldown = Math.Max(
            CurrentWorld.Player.Value.Current.WalkCooldown - _gameContext.TimeContext.Delta,
            0);

        CurrentWorld.CameraPositionLerpT = MathF.Min(
            CurrentWorld.CameraPositionLerpT + (
                    (1.0F / (float)CurrentWorld.Player.Value.Default.WalkCooldown)
                    * (float)_gameContext.TimeContext.Delta),
            1);
    }

    public void UpdateChests()
    {
        if (CurrentWorld is null)
        {
            return;
        }

        for (int i = 0; i < CurrentWorld.Chests.Count; i++)
        {
            var chest = CurrentWorld.Chests[i];

            if (chest.Value.CurrentStatus == Chest.Status.Opening)
            {
                chest.Value.Openness += (float)_gameContext.TimeContext.Delta * 2;

                if (chest.Value.Openness >= 1)
                {
                    chest.Value.Openness = 1;
                    chest.Value.CurrentStatus = Chest.Status.Opened;
                    CurrentWorld.OpenChest(i);
                    chest.Value.Inventory.TransferTo(CurrentWorld.Player.Value.Inventory);
                    _gameContext.AudioService.PlaySoundEffect(SoundEffect.Item);
                }
            }

            CurrentWorld.Chests[i] = chest;
        }
    }

    public bool TryToInteractWithChest(Position at)
    {
        if (CurrentWorld is null)
        {
            return false;
        }

        if (!CurrentWorld.ChestMap.TryGetValue(at, out int chestID))
        {
            return false;
        }

        var spawnedChest = CurrentWorld.Chests[chestID];

        if (spawnedChest.Value.CurrentStatus != Chest.Status.Idle)
        {
            return false;
        }

        spawnedChest.Value.CurrentStatus = Chest.Status.Opening;
        CurrentWorld.Chests[chestID] = spawnedChest;
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.OpenChest);

        return true;
    }

    public void BleedOutPlayer()
    {
        if (CurrentWorld is not null)
        {
            var bleedfreq = (float)_gameContext.TimeContext.Delta * 3;
            var player = CurrentWorld.Player.Value;

            if (player.RunningHealth < player.Current.Health)
            {
                player.RunningHealth += bleedfreq;

                if (player.RunningHealth >= player.Current.Health)
                {
                    player.RunningHealth = player.Current.Health;
                }
            }
            else if (player.RunningHealth > player.Current.Health)
            {
                player.RunningHealth -= bleedfreq;

                if (player.RunningHealth <= player.Current.Health)
                {
                    player.RunningHealth = player.Current.Health;
                }
            }

            CurrentWorld.Player.Value = player;
        }
    }
}
