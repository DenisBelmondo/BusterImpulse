using Raylib_cs.BleedingEdge;
using static Raylib_cs.BleedingEdge.Raylib;

namespace Belmondo.FightFightDanger.Raylib_cs;

public sealed class RaylibInputService : IInputService, IThinker
{
    public struct InputState
    {
        public bool IsPressed;
        public bool WasJustPressed;
        public bool WasJustReleased;
    }

    public struct InputRecord
    {
        public KeyboardKey? KeyboardKey1;
        public KeyboardKey? KeyboardKey2;
        public MouseButton? MouseButton1;
        public MouseButton? MouseButton2;
        public GamepadButton? GamepadButton1;
        public GamepadButton? GamepadButton2;
        public int GamepadButton1GamepadIndex;
        public int GamepadButton2GamepadIndex;
    }

    private static readonly int NUM_INPUT_STATES = Enum.GetNames<InputAction>().Length;
    private static readonly InputRecord[] _inputMap = new InputRecord[NUM_INPUT_STATES];

    private readonly InputState[] _inputStates = new InputState[NUM_INPUT_STATES];

    public IReadOnlyCollection<InputState> InputStates => _inputStates;

    public RaylibInputService()
    {
        InitInputMapDefaults();
    }

    private void InitInputMapDefaults()
    {
        _inputMap[(int)InputAction.MoveForward]       = new() { KeyboardKey1 = KeyboardKey.W, KeyboardKey2 = KeyboardKey.Up, };
        _inputMap[(int)InputAction.MoveRight]         = new() { KeyboardKey1 = KeyboardKey.D, };
        _inputMap[(int)InputAction.MoveBack]          = new() { KeyboardKey1 = KeyboardKey.S, KeyboardKey2 = KeyboardKey.Down, };
        _inputMap[(int)InputAction.MoveLeft]          = new() { KeyboardKey1 = KeyboardKey.A, };
        _inputMap[(int)InputAction.LookLeft]          = new() { KeyboardKey1 = KeyboardKey.Left, };
        _inputMap[(int)InputAction.LookRight]         = new() { KeyboardKey1 = KeyboardKey.Right, };
        _inputMap[(int)InputAction.Confirm]           = new() { KeyboardKey1 = KeyboardKey.Space, };
        _inputMap[(int)InputAction.Cancel]            = new() { KeyboardKey1 = KeyboardKey.Backspace, };
        _inputMap[(int)InputAction.BattleAttack]      = new() { KeyboardKey1 = KeyboardKey.P, };
        _inputMap[(int)InputAction.BattleDefend]      = new() { KeyboardKey1 = KeyboardKey.H, };
        _inputMap[(int)InputAction.BattleRun]         = new() { KeyboardKey1 = KeyboardKey.R, };
        _inputMap[(int)InputAction.BattleItem]        = new() { KeyboardKey1 = KeyboardKey.I, };
        _inputMap[(int)InputAction.DebugBattleScreen] = new() { KeyboardKey1 = KeyboardKey.B, };
    }

    public void Update()
    {
        for (int i = 0; i < NUM_INPUT_STATES; i++)
        {
            var inputRecord = _inputMap[i];

            var currentlyIsPressed = false
                || (inputRecord.KeyboardKey1 is KeyboardKey keyboardKey1 && IsKeyDown(keyboardKey1))
                || (inputRecord.KeyboardKey2 is KeyboardKey keyboardKey2 && IsKeyDown(keyboardKey2))
                || (inputRecord.MouseButton1 is MouseButton mouseButton1 && IsMouseButtonDown(mouseButton1))
                || (inputRecord.MouseButton2 is MouseButton mouseButton2 && IsMouseButtonDown(mouseButton2))
                || (inputRecord.GamepadButton1 is GamepadButton gamepadButton1 && IsGamepadButtonDown(inputRecord.GamepadButton1GamepadIndex, gamepadButton1))
                || (inputRecord.GamepadButton2 is GamepadButton gamepadButton2 && IsGamepadButtonDown(inputRecord.GamepadButton2GamepadIndex, gamepadButton2));

            var wasJustPressed = !_inputStates[i].IsPressed && currentlyIsPressed;
            var wasJustReleased = _inputStates[i].IsPressed && !currentlyIsPressed;

            _inputStates[i].IsPressed = currentlyIsPressed;
            _inputStates[i].WasJustPressed = wasJustPressed;
            _inputStates[i].WasJustReleased = wasJustReleased;
        }
    }

    public bool ActionIsPressed(InputAction action) => _inputStates[(int)action].IsPressed;
    public bool ActionWasJustPressed(InputAction action) => _inputStates[(int)action].WasJustPressed;
    public bool ActionWasJustReleased(InputAction action) => _inputStates[(int)action].WasJustReleased;
}
