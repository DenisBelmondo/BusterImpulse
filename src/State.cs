namespace Belmondo.FightFightDanger;

public class State
{
	public TimeSpan WaitTimeRemaining;
	public string? Name;
	public Func<bool> Enter = () => true;
	public Func<bool>? Update = null;
	public Action? Exit = null;
}

public class StateQueue
{
	private TimeSpan _lag;
	private DateTime _oldTime = DateTime.Now;
	private bool _shouldEnter = true;

	public Queue<State> Queue = [];

	private void Step()
	{
		if (Queue.Count == 0)
		{
			_shouldEnter = true;
			return;
		}

		State currentState = Queue.Peek();

		if (_shouldEnter)
		{
			bool success = (currentState.Enter?.Invoke()).GetValueOrDefault();

			if (!success)
			{
				_shouldEnter = true;
				Queue.Dequeue();
				return;
			}

			_shouldEnter = false;
		}
	
		if (!(currentState.Update?.Invoke()).GetValueOrDefault())
		{
			currentState.Exit?.Invoke();
			_shouldEnter = true;
			Queue.Dequeue();
		}
	}

	public void Update()
	{
		DateTime now = DateTime.Now;
		TimeSpan timeElapsed = now - _oldTime;

		_lag += timeElapsed;
		_oldTime = now;

		while (_lag >= timeElapsed)
		{
			if (Queue.TryPeek(out State? currentState))
			{
				if (currentState.WaitTimeRemaining > TimeSpan.Zero)
				{
					currentState.WaitTimeRemaining -= timeElapsed;
				}
				else
				{
					Step();
				}
			}

			_lag -= timeElapsed;
		}
	}
}

public class StateQueueBuilder
{
	private TimeSpan _nextStateTimeRemaining;
	public StateQueue StateQueue = new();

	public StateQueueBuilder Then(in State nextState)
	{
		var nextState_ = nextState;

		nextState_.WaitTimeRemaining = _nextStateTimeRemaining;
		StateQueue.Queue.Enqueue(nextState_);

		return this;
	}

	public StateQueueBuilder Wait(TimeSpan timeSpan)
	{
		_nextStateTimeRemaining = timeSpan;
		return this;
	}

	public StateQueue Build()
	{
		return StateQueue;
	}
}
