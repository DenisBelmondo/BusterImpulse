namespace Belmondo.FightFightDanger;

public class TimerContext(TimeContext timeContext) : IProgressTracker, IThinker, IResettable
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

    public static double GetProgress(params TimerContext[] timers)
    {
        double durationSeconds = 0;
        double secondsRemaining = 0;

        foreach (var timer in timers)
        {
            durationSeconds += timer.DurationSeconds;
            secondsRemaining += timer.SecondsRemaining;
        }

        if (durationSeconds == 0)
        {
            return 0;
        }

        return (durationSeconds - secondsRemaining) / durationSeconds;
    }

    public double GetProgress()
    {
        if (DurationSeconds == 0)
        {
            return 0;
        }

        return (DurationSeconds - SecondsRemaining) / DurationSeconds;
    }

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
