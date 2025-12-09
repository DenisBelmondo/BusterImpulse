namespace Belmondo.FightFightDanger;

public sealed class OldState
{
    public enum Flow
    {
        Continue,
        Stop,
        Reset,
        Goto,
    }

    public struct Result(Flow flow, OldState? nextState = null)
    {
        public Flow Flow = flow;
        public OldState? NextState = nextState;
    }

    public static Result Continue => new(Flow.Continue);
    public static Result Stop => new(Flow.Stop);
    public static Result Reset => new(Flow.Reset);
    public static Result Goto(OldState nextState) => new(Flow.Goto, nextState);

    public Action? EnterFunction;
    public Func<Result>? UpdateFunction;
    public Action? ExitFunction;
}

public sealed class OldStateAutomaton
{
    public class StateChangeEventArgs(OldState nextState) : EventArgs
    {
        public OldState NewState = nextState;
    }

    public event EventHandler? StateChanged;

    private bool _hasEntered;
    public OldState? CurrentState;

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

        OldState.Result? maybeResult = CurrentState.UpdateFunction?.Invoke();

        if (maybeResult is OldState.Result result)
        {
            switch (result.Flow)
            {
                case OldState.Flow.Continue:
                    return;
                case OldState.Flow.Stop:
                    CurrentState = null;
                    return;
                case OldState.Flow.Reset:
                    _hasEntered = false;
                    return;
                case OldState.Flow.Goto:
                    {
                        if (result.NextState is OldState nextState)
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
