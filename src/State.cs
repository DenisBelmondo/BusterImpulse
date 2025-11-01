namespace Belmondo;

public static partial class FightFightDanger
{
	public class State
	{
		public Action? EnterFunction;
		public Func<Lazy<State?>>? UpdateFunction;
		public Action? ExitFunction;
	}

	public class StateAutomaton
	{
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
				CurrentState!.EnterFunction?.Invoke();
				_hasEntered = true;
			}

			Lazy<State?>? nextState = CurrentState.UpdateFunction?.Invoke();

			if (nextState is not null && nextState.Value is not null)
			{
				CurrentState!.ExitFunction?.Invoke();
				CurrentState = nextState.Value;
				_hasEntered = false;
			}
		}
	}
}
