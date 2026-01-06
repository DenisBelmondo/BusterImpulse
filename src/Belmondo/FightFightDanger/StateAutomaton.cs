namespace Belmondo.FightFightDanger;

public class StateAutomaton<TContext, TStateEnum> where TStateEnum : struct, IComparable
{
    public enum Flow
    {
        Continue,
        Stop,
        Goto,
    }

    public struct Result
    {
        public Flow Flow;
        public TStateEnum? NextState;

        public static Result Continue => new()
        {
            Flow = Flow.Continue,
        };

        public static Result Stop = new()
        {
            Flow = Flow.Stop,
        };

        public static Result Goto(TStateEnum nextState) => new()
        {
            Flow = Flow.Goto,
            NextState = nextState,
        };
    }

    public delegate Result StateFunction(TContext context, TStateEnum currentState);

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

    private bool HandleTransition(TContext context, TStateEnum currentState, in Result result)
    {
        TStateEnum? maybeNextState = result.NextState;
        bool shouldCallExitFunction = false;
        bool isStillRunning = true;

        switch (result.Flow)
        {
        case Flow.Stop:
            shouldCallExitFunction = true;
            isStillRunning = false;
            Stop();

            break;
        case Flow.Goto:
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
        Result? maybeResult = null;

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
