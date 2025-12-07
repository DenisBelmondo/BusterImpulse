namespace Belmondo;

public static partial class FightFightDanger
{
    public interface IInputService
    {
        public bool ActionWasJustPressed(int action);
        public bool ActionWasJustReleased(int action);
        public bool ActionIsPressed(int action);
    }
}
