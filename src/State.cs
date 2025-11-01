namespace Belmondo.FightFightDanger;

public class State
{
	public Action? EnterFunction;
	public Func<Lazy<State?>>? UpdateFunction;
	public Action? ExitFunction;
}

public class StateAutomaton
{
	private bool _hasEntered;
	public Lazy<State?>? CurrentState;

	public void Update()
	{
		if (CurrentState is null || !CurrentState.IsValueCreated)
		{
			return;
		}

		if (!_hasEntered)
		{
			CurrentState!.Value?.EnterFunction?.Invoke();
			_hasEntered = true;
		}

		Lazy<State?>? nextState = CurrentState?.Value?.UpdateFunction?.Invoke();

		if (nextState is not null && nextState.IsValueCreated && nextState.Value is not null)
		{
			CurrentState!.Value?.ExitFunction?.Invoke();
			CurrentState = nextState;
		}
	}
}
