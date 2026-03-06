namespace Belmondo.FightFightDanger;

public enum StateFlowFlag
{
    Continue,
    Stop,
    Goto,
}

public struct StateFlowResult<TStateEnum> where TStateEnum : struct, IComparable
{
    public StateFlowFlag Flag;
    public TStateEnum? NextState;

    public static readonly StateFlowResult<TStateEnum> Continue = new()
    {
        Flag = StateFlowFlag.Continue,
    };

    public static readonly StateFlowResult<TStateEnum> Stop = new()
    {
        Flag = StateFlowFlag.Stop,
    };

    public static StateFlowResult<TStateEnum> Goto(TStateEnum nextState) => new()
    {
        Flag = StateFlowFlag.Goto,
        NextState = nextState,
    };
}

public class StateAutomaton<TContext, TStateEnum> where TStateEnum : struct, IComparable
{
    public delegate StateFlowResult<TStateEnum> StateFunction(TContext context, TStateEnum currentState);

    public StateFunction? EnterFunction;
    public StateFunction? UpdateFunction;
    public StateFunction? ExitFunction;

    private bool _hasEntered;
    public Stack<TStateEnum> Stack = [];

    public TStateEnum? CurrentState
    {
        get
        {
            if (Stack.TryPeek(out TStateEnum state))
            {
                return state;
            }

            return null;
        }
    }

    public TStateEnum? PreviousState { get; private set; }

    private bool HandleTransition(TContext context, TStateEnum currentState, in StateFlowResult<TStateEnum> result)
    {
        TStateEnum? maybeNextState = result.NextState;
        bool shouldCallExitFunction = false;
        bool isStillRunning = true;

        switch (result.Flag)
        {
            case StateFlowFlag.Stop:
                shouldCallExitFunction = true;
                isStillRunning = false;
                Stop();

                break;
            case StateFlowFlag.Goto:
                if (maybeNextState is TStateEnum nextState)
                {
                    shouldCallExitFunction = true;
                    ChangeState(nextState);
                }

                break;
        }

        if (shouldCallExitFunction)
        {
            ExitFunction?.Invoke(context, currentState);
        }

        return isStillRunning;
    }

    public bool IsProcessingState(TStateEnum state)
    {
        if (CurrentState is null)
        {
            return false;
        }

        return CurrentState.Equals(state);
    }

    public bool IsRunning() => CurrentState is not null;

    public void ChangeState(TStateEnum newState)
    {
        if (Stack.Count > 0)
        {
            Stack.Pop();
        }

        Stack.Push(newState);
        _hasEntered = false;
    }

    public void Push(TStateEnum newState)
    {
        Stack.Push(newState);
        _hasEntered = false;
    }

    public void Pop()
    {
        Stack.Pop();
    }

    public void Stop()
    {
        Stack.Clear();
        _hasEntered = false;
    }

    public void Update(TContext context)
    {
        PreviousState = CurrentState;

        if (CurrentState is null)
        {
            return;
        }

        TStateEnum currentState = CurrentState.Value;
        StateFlowResult<TStateEnum>? maybeResult = null;

        if (!_hasEntered)
        {
            maybeResult = EnterFunction?.Invoke(context, currentState);

            if (maybeResult is not null && !HandleTransition(context, currentState, maybeResult.Value))
            {
                return;
            }

            _hasEntered = true;
        }

        maybeResult = UpdateFunction?.Invoke(context, currentState);

        if (maybeResult is not null)
        {
            HandleTransition(context, currentState, maybeResult.Value);
        }
    }
}
