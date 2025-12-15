namespace Belmondo.FightFightDanger;

public enum StateFlow
{
    Continue,
    Stop,
    Reset,
    Goto,
}

public sealed class State<T>
{
    public struct Result(StateFlow flow, State<T>? nextState = null)
    {
        public StateFlow Flow = flow;
        public State<T>? NextState = nextState;
    }

    public delegate void EnterDelegate(T arg);
    public delegate Result UpdateDelegate(T arg);
    public delegate void ExitDelegate(T arg);

    public static readonly State<T> Empty = new();

    public static Result Continue => new(StateFlow.Continue);
    public static Result Stop => new(StateFlow.Stop);
    public static Result Reset => new(StateFlow.Reset);
    public static Result Goto(State<T> nextState) => new(StateFlow.Goto, nextState);

    public EnterDelegate? EnterFunction;
    public UpdateDelegate? UpdateFunction;
    public ExitDelegate? ExitFunction;
}

public sealed class StateAutomaton<T>
{
    public class StateChangeEventArgs(State<T> nextState) : EventArgs
    {
        public State<T> NewState = nextState;
    }

    public event EventHandler? StateChanged;

    private bool _hasEntered;
    public State<T>? PreviousState;
    public State<T>? CurrentState;

    public void Update(T arg)
    {
        PreviousState = CurrentState;

        if (CurrentState is null)
        {
            return;
        }

        if (!_hasEntered)
        {
            _hasEntered = true;
            CurrentState.EnterFunction?.Invoke(arg);
            StateChanged?.Invoke(this, new StateChangeEventArgs(CurrentState));
        }

        State<T>.Result? maybeResult = CurrentState.UpdateFunction?.Invoke(arg);

        if (maybeResult is State<T>.Result result)
        {
            switch (result.Flow)
            {
                case StateFlow.Continue:
                    return;
                case StateFlow.Stop:
                    CurrentState = State<T>.Empty;
                    return;
                case StateFlow.Reset:
                    _hasEntered = false;
                    return;
                case StateFlow.Goto:
                    {
                        if (result.NextState is State<T> nextState)
                        {
                            CurrentState.ExitFunction?.Invoke(arg);
                            CurrentState = nextState;
                            _hasEntered = false;
                        }
                    }
                    return;
            }
        }
    }

    public void ChangeState(State<T> state)
    {
        CurrentState = state;
        _hasEntered = false;
    }
}
