namespace Belmondo;

public static partial class FightFightDanger
{
	public class State
	{
        public static readonly Lazy<State?> None = new(() => null);

        public Action? EnterFunction;
		public Func<Lazy<State?>>? UpdateFunction;
		public Action? ExitFunction;
	}

	public class StateAutomaton
	{
		public class StateChangeEventArgs(Lazy<State> nextState) : EventArgs
		{
            public Lazy<State> NewState = nextState;
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
				CurrentState!.EnterFunction?.Invoke();
				_hasEntered = true;
			}

			Lazy<State?>? nextState = CurrentState.UpdateFunction?.Invoke();

			if (nextState is not null && nextState.Value is not null)
			{
				CurrentState!.ExitFunction?.Invoke();
				CurrentState = nextState.Value;
				_hasEntered = false;
                StateChanged?.Invoke(this, new StateChangeEventArgs(nextState!));
            }
		}
	}
}
