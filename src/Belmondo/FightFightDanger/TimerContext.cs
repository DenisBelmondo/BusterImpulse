namespace Belmondo.FightFightDanger;

public class TimerContext(TimeContext timeContext) : IThinker, IResettable
{
    public enum Status
    {
        Stopped,
        Running,
    }

    public event Action? TimedOut;

    public Status CurrentStatus;
    public double DurationSeconds;
    public double SecondsRemaining;
    public bool JustStarted;
    public bool JustTimedOut;

    public double GetProgress() => (DurationSeconds - SecondsRemaining) / DurationSeconds;

    public void Start(double seconds = -1)
    {
        if (seconds > 0)
        {
            DurationSeconds = seconds;
        }

        SecondsRemaining = DurationSeconds;
        CurrentStatus = Status.Running;
        JustStarted = true;
    }

    public void Update()
    {
        JustTimedOut = false;

        if (CurrentStatus != Status.Running)
        {
            JustStarted = false;
            return;
        }

        SecondsRemaining -= timeContext.Delta;

        if (SecondsRemaining <= 0)
        {
            SecondsRemaining = 0;
            CurrentStatus = Status.Stopped;
            JustTimedOut = true;
            TimedOut?.Invoke();
        }

        JustStarted = false;
    }

    public void Reset()
    {
        CurrentStatus = default;
        DurationSeconds = default;
        SecondsRemaining = default;
    }
}
