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

    public void Start(double seconds = -1)
    {
        if (seconds > 0)
        {
            DurationSeconds = seconds;
        }

        SecondsRemaining = DurationSeconds;
        CurrentStatus = Status.Running;
    }

    public void Update()
    {
        if (CurrentStatus == Status.Stopped)
        {
            return;
        }

        SecondsRemaining -= timeContext.Delta;

        if (SecondsRemaining <= 0)
        {
            SecondsRemaining = 0;
            CurrentStatus = Status.Stopped;
            TimedOut?.Invoke();
        }
    }

    public void Reset()
    {
        CurrentStatus = default;
        DurationSeconds = default;
        SecondsRemaining = default;
    }
}
