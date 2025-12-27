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

        public TimerContext FadeOutTimer = new(timeContext);
        public TimerContext StickAroundTimer = new(timeContext);
        public TimerContext FadeInTimer = new(timeContext);
        public double FadeT;

        public TransitionStateAutomaton StateAutomaton = new()
        {
            EnterFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                    {
                        self.FadeOutTimer.Start(1);
                        break;
                    }

                    case State.StickingAround:
                    {
                        self.StickAroundTimer.Start(0.5);
                        break;
                    }

                    case State.FadingIn:
                    {
                        self.FadeInTimer.Start(1);
                        break;
                    }
                }

                return TransitionStateAutomaton.Result.Continue;
            },

            UpdateFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                    {
                        self.FadeT = self.FadeOutTimer.GetProgress();

                        if (self.FadeOutTimer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.StickingAround);
                        }

                        break;
                    }

                    case State.StickingAround:
                    {
                        if (self.FadeOutTimer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Goto(State.FadingIn);
                        }

                        break;
                    }

                    case State.FadingIn:
                    {
                        self.FadeT = 1 + self.FadeInTimer.GetProgress();

                        if (self.FadeInTimer.CurrentStatus == TimerContext.Status.Stopped)
                        {
                            return TransitionStateAutomaton.Result.Stop;
                        }

                        break;
                    }
                }

                return TransitionStateAutomaton.Result.Continue;
            },

            ExitFunction = static (self, currentState) =>
            {
                switch (currentState)
                {
                    case State.FadingOut:
                    {
                        FadedOut?.Invoke(self);
                        break;
                    }

                    case State.FadingIn:
                    {
                        self.FadeOutTimer.Reset();
                        self.StickAroundTimer.Reset();
                        self.FadeInTimer.Reset();

                        break;
                    }
                }

                return TransitionStateAutomaton.Result.Continue;
            },
        };

        public void Update()
        {
            StateAutomaton.Update(this);
            FadeOutTimer.Update();
            StickAroundTimer.Update();
            FadeInTimer.Update();
        }
    }

    public class MenuContext : IResettable
    {
        public static Menu MainMenu = new()
        {
            Items = [
                new()
                {
                    Name = "Snacks",
                },

                new()
                {
                    Name = "Charms",
                },

                new()
                {
                    Name = "Quit",
                },
            ],
        };

        public Stack<Menu> MenuStack = [];

        public void Reset()
        {
            MainMenu.Reset();
            MenuStack.Clear();
        }
    }

    private readonly GameContext _gameContext;

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
            {
                self.Log.Clear();
                self.CurrentRenderHint = RenderHint.Exploring;
                self._gameContext.AudioService.ChangeMusic(MusicTrack.WanderingStage1);

                break;
            }

            case State.Transitioning:
            {
                self.CurrentTransitionContext.StateAutomaton.ChangeState(TransitionContext.State.FadingOut);
                break;
            }

            case State.Battling:
            {
                self.CurrentRenderHint = RenderHint.Battling;
                self._gameContext.AudioService.ChangeMusic(MusicTrack.Battle);
                break;
            }
        }

        return PlaysimStateResult.Continue;
    }

    private static PlaysimStateResult UpdateFunction(Game self, State currentState)
    {
        switch (currentState)
        {
            case State.Exploring:
            {
                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.DebugBattleScreen))
                {
                    self.StartBattle();
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Cancel))
                {
                    self.OpenMenu();
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
            }

            case State.Menu:
            {   if (self.CurrentMenuContext.MenuStack.TryPeek(out Menu? menu))
                {
                    if (self._gameContext.InputService.ActionWasJustPressed(InputAction.MoveBack))
                    {
                        menu.CurrentItem += 1;

                        if (menu.CurrentItem >= menu.Items.Count)
                        {
                            menu.CurrentItem = 0;
                        }

                        self._gameContext.AudioService.PlaySoundEffect(SoundEffect.UIFocus);
                    }
                    else if (self._gameContext.InputService.ActionWasJustPressed(InputAction.MoveForward))
                    {
                        menu.CurrentItem -= 1;

                        if (menu.CurrentItem < 0)
                        {
                            menu.CurrentItem = menu.Items.Count - 1;
                        }

                        self._gameContext.AudioService.PlaySoundEffect(SoundEffect.UIFocus);
                    }
                    else if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Confirm))
                    {
                        self._gameContext.AudioService.PlaySoundEffect(SoundEffect.UIConfirm);
                    }
                }

                if (self._gameContext.InputService.ActionWasJustPressed(InputAction.Cancel))
                {
                    self.CloseMenu();
                    self._gameContext.AudioService.PlaySoundEffect(SoundEffect.UICancel);
                }

                break;
            }

            case State.Transitioning:
            {
                if (!self.CurrentTransitionContext.StateAutomaton.IsRunning())
                {
                    return PlaysimStateResult.Goto(State.Battling);
                }

                break;
            }

            case State.Battling:
            {
                if (self.World is not null)
                {
                    self.World.Player.Value.RunningHealth += MathF.Sign(self.World.Player.Value.Current.Health - self.World.Player.Value.RunningHealth) * ((float)self._gameContext.TimeContext.Delta * 3);
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

    public void StartBattle()
    {
        Battle?.Reset();
        StateAutomaton.ChangeState(State.Transitioning);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.BattleStart);
    }

    public void OpenMenu()
    {
        CurrentMenuContext.MenuStack.Clear();
        CurrentMenuContext.MenuStack.Push(MenuContext.MainMenu);
        StateAutomaton.ChangeState(State.Menu);
        _gameContext.AudioService.PlaySoundEffect(SoundEffect.UIConfirm);
    }

    public void CloseMenu()
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
