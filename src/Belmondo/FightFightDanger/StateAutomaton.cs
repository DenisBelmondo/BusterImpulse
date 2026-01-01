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


        if (!_hasEntered)
        {
            EnterFunction?.Invoke(context, currentState);
            _hasEntered = true;
        }

        Result? maybeResult = UpdateFunction?.Invoke(context, currentState);

        if (maybeResult is Result result)
        {
            TStateEnum? maybeNextState = result.NextState;

            switch (result.Flow)
            {
            case Flow.Continue:
                break;
            case Flow.Stop:
                ExitFunction?.Invoke(context, currentState);
                Stop();
                break;
            case Flow.Goto:
                if (maybeNextState is TStateEnum nextState)
                {
                    ExitFunction?.Invoke(context, currentState);
                    ChangeState(nextState);
                }
                break;
            }
        }
    }
}
