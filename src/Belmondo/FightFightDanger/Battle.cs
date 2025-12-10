namespace Belmondo.FightFightDanger;

public class Battle
{
    public GameContext GameContext;

    public StateAutomaton PlayerDodgeStateAutomaton = new();
    public float PlayerDodgeT;

    public State? PlayerReadyState;
    public State? PlayerDodgeState;

    public Battle(GameContext gameContext)
    {
        GameContext = gameContext;

        PlayerReadyState = new()
        {
            UpdateFunction = () =>
            {
                var shouldDodge = (true
                    || gameContext.InputService.ActionWasJustPressed(InputAction.MoveLeft)
                    || gameContext.InputService.ActionWasJustPressed(InputAction.MoveRight));

                if (shouldDodge)
                {
                    return State.Goto(PlayerDodgeState!);
                }

                return State.Continue;
            },
        };

        PlayerDodgeState = new()
        {
            EnterFunction = () =>
            {
                PlayerDodgeT = 0.5f;
            },

            UpdateFunction = () =>
            {
                PlayerDodgeT -= (float)GameContext.Delta;

                if (PlayerDodgeT <= 0)
                {
                    return State.Goto(PlayerReadyState);
                }

                return State.Continue;
            },
        };
    }

    public void Update()
    {
        PlayerDodgeStateAutomaton.Update();
    }
}
