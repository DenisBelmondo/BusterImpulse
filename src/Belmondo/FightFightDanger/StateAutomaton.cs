namespace Belmondo.FightFightDanger;

public partial class StateAutomaton<TContext, TStateEnum> where TStateEnum : struct, Enum
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
    public TStateEnum? CurrentState;
    public TStateEnum? PreviousState { get; private set; }

    public bool IsProcessingState(TStateEnum state)
    {
        if (CurrentState is null)
        {
            return false;
        }

        return CurrentState.Value.Equals(state);
    }

    public bool IsRunning() => CurrentState is not null;

    public void ChangeState(TStateEnum newState)
    {
        CurrentState = newState;
        _hasEntered = false;
    }

    public void Stop()
    {
        CurrentState = null;
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
                _hasEntered = false;
                CurrentState = null;
                break;
            case Flow.Goto:
                if (maybeNextState is TStateEnum nextState)
                {
                    ExitFunction?.Invoke(context, currentState);
                    CurrentState = nextState;
                    _hasEntered = false;
                }
                break;
            }
        }
    }
}
