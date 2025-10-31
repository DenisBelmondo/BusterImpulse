namespace Belmondo.FightFightDanger;

public class State
{
	public Action? EnterFunction;
	public Func<State?>? UpdateFunction;
	public Action? ExitFunction;
}

public class StateAutomaton(State initialState)
{
	private bool _hasEntered;
	public State CurrentState = initialState;

	public void Update()
	{
		if (!_hasEntered)
		{
			CurrentState.EnterFunction?.Invoke();
			_hasEntered = true;
		}

		State? nextState = CurrentState.UpdateFunction?.Invoke();

		if (nextState is not null)
		{
			CurrentState.ExitFunction?.Invoke();
			CurrentState = nextState;
		}
	}
}
