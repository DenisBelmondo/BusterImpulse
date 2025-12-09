using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger.Raylib_cs;

public sealed class RaylibInputService : IInputService
{
    public bool ActionIsPressed(InputAction action) => action switch
    {
        InputAction.MoveForward => IsKeyDown(KeyboardKey.W),
        InputAction.MoveRight => IsKeyDown(KeyboardKey.D),
        InputAction.MoveBack => IsKeyDown(KeyboardKey.S),
        InputAction.MoveLeft => IsKeyDown(KeyboardKey.A),
        InputAction.LookRight => IsKeyDown(KeyboardKey.Right),
        InputAction.LookLeft => IsKeyDown(KeyboardKey.Left),
        InputAction.Confirm => IsKeyDown(KeyboardKey.Space),
        InputAction.Cancel => IsKeyDown(KeyboardKey.Backspace),
        InputAction.DebugBattleScreen => IsKeyDown(KeyboardKey.B),
        InputAction.BattleAttack => IsKeyDown(KeyboardKey.A),
        InputAction.BattleDefend => IsKeyDown(KeyboardKey.D),
        InputAction.BattleRun => IsKeyDown(KeyboardKey.R),
        InputAction.BattleItem => IsKeyDown(KeyboardKey.I),
        _ => false,
    };

    public bool ActionWasJustPressed(InputAction action) => action switch
    {
        InputAction.MoveForward => IsKeyPressed(KeyboardKey.W),
        InputAction.MoveRight => IsKeyPressed(KeyboardKey.D),
        InputAction.MoveBack => IsKeyPressed(KeyboardKey.S),
        InputAction.MoveLeft => IsKeyPressed(KeyboardKey.A),
        InputAction.LookRight => IsKeyPressed(KeyboardKey.Right),
        InputAction.LookLeft => IsKeyPressed(KeyboardKey.Left),
        InputAction.Confirm => IsKeyPressed(KeyboardKey.Space),
        InputAction.Cancel => IsKeyPressed(KeyboardKey.Backspace),
        InputAction.DebugBattleScreen => IsKeyPressed(KeyboardKey.B),
        InputAction.BattleAttack => IsKeyPressed(KeyboardKey.A),
        InputAction.BattleDefend => IsKeyPressed(KeyboardKey.D),
        InputAction.BattleRun => IsKeyPressed(KeyboardKey.R),
        InputAction.BattleItem => IsKeyPressed(KeyboardKey.I),
        _ => false,
    };

    public bool ActionWasJustReleased(InputAction action) => action switch
    {
        InputAction.MoveForward => IsKeyReleased(KeyboardKey.W),
        InputAction.MoveRight => IsKeyReleased(KeyboardKey.D),
        InputAction.MoveBack => IsKeyReleased(KeyboardKey.S),
        InputAction.MoveLeft => IsKeyReleased(KeyboardKey.A),
        InputAction.LookRight => IsKeyReleased(KeyboardKey.Right),
        InputAction.LookLeft => IsKeyReleased(KeyboardKey.Left),
        InputAction.Confirm => IsKeyReleased(KeyboardKey.Space),
        InputAction.Cancel => IsKeyReleased(KeyboardKey.Backspace),
        InputAction.DebugBattleScreen => IsKeyReleased(KeyboardKey.B),
        InputAction.BattleAttack => IsKeyReleased(KeyboardKey.A),
        InputAction.BattleDefend => IsKeyReleased(KeyboardKey.D),
        InputAction.BattleRun => IsKeyReleased(KeyboardKey.R),
        InputAction.BattleItem => IsKeyPressed(KeyboardKey.I),
        _ => false,
    };
}
