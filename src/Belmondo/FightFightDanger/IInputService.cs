namespace Belmondo.FightFightDanger;

public interface IInputService
{
    bool ActionWasJustPressed(InputAction action);
    bool ActionWasJustReleased(InputAction action);
    bool ActionIsPressed(InputAction action);
}
