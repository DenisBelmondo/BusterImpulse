namespace Belmondo.FightFightDanger;

public interface IInputService
{
    public bool ActionWasJustPressed(InputAction action);
    public bool ActionWasJustReleased(InputAction action);
    public bool ActionIsPressed(InputAction action);
}
