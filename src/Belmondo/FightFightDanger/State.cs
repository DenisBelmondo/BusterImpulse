namespace Belmondo.FightFightDanger;

public sealed class State
{
    public enum Flow
    {
        Continue,
        Stop,
        Reset,
        Goto,
    }

    public struct Result(Flow flow, State? nextState = null)
    {
        public Flow Flow = flow;
        public State? NextState = nextState;
    }

    public static Result Continue => new(Flow.Continue);
    public static Result Stop => new(Flow.Stop);
    public static Result Reset => new(Flow.Reset);
    public static Result Goto(State nextState) => new(Flow.Goto, nextState);

    public Action? EnterFunction;
    public Func<Result>? UpdateFunction;
    public Action? ExitFunction;
}

public sealed class StateAutomaton
{
    public class StateChangeEventArgs(State nextState) : EventArgs
    {
        public State NewState = nextState;
    }

    public event EventHandler? StateChanged;

    private bool _hasEntered;
    public State? CurrentState;

    public void Update()
    {
        if (CurrentState is null)
        {
            return;
        }

        if (!_hasEntered)
        {
            _hasEntered = true;
            CurrentState.EnterFunction?.Invoke();
            StateChanged?.Invoke(this, new StateChangeEventArgs(CurrentState));
        }

        State.Result? maybeResult = CurrentState.UpdateFunction?.Invoke();

        if (maybeResult is State.Result result)
        {
            switch (result.Flow)
            {
                case State.Flow.Continue:
                    return;
                case State.Flow.Stop:
                    CurrentState = null;
                    return;
                case State.Flow.Reset:
                    _hasEntered = false;
                    return;
                case State.Flow.Goto:
                    {
                        if (result.NextState is State nextState)
                        {
                            CurrentState.ExitFunction?.Invoke();
                            CurrentState = nextState;
                            _hasEntered = false;
                        }
                    }
                    return;
            }
        }
    }
}
