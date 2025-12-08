namespace Belmondo;

class Tween<TValue> where TValue : struct
{
    public enum Status
    {
        Uninitialized,
        Initialized,
        Running,
        Finished,
    }

    public struct State
    {
        public TValue From;
        public TValue To;
        public float CurrentT;
        public Status CurrentStatus;
        public TValue CurrentValue;
    }

    public required Func<TValue, TValue, float, TValue> LerpFunction;
    public State CurrentState;

    public void Initialize(TValue from, TValue to) => CurrentState = new()
    {
        From = from,
        To = to,
    };

    public void Update(double deltaTime)
    {
        if (CurrentState.CurrentStatus == Status.Running)
        {
            CurrentState.CurrentT += (float)deltaTime;
            CurrentState.CurrentT = Math.Clamp(CurrentState.CurrentT, 0, 1);
            CurrentState.CurrentValue = LerpFunction(CurrentState.From, CurrentState.To, CurrentState.CurrentT);

            if (CurrentState.CurrentT >= 1)
            {
                CurrentState.CurrentStatus = Status.Finished;
            }
        }
    }
}
