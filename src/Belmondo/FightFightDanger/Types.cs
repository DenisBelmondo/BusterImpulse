using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Belmondo.FightFightDanger;

public enum BinaryState
{
    Off,
    On,
}

public enum CharmType
{
    ElephantStatue,
    FourLeafClover,
    GoldenGrapes,
    Milagro,
    RabbitsFoot,
    Scarab,
}

public struct Chest()
{
    public enum Status
    {
        Idle,
        Opening,
        Opened,
    }

    public Inventory Inventory = new();
    public float Openness;
    public Status CurrentStatus;
}

public struct DialogState()
{
    public string? TargetLine;
    public StringBuilder RunningLine = new();
    public float CurrentCharacterIndex;
}

public class Inventory
{
    public Dictionary<CharmType, int> Charms = [];
    public Dictionary<SnackType, int> Snacks = [];

    public void TransferTo(Inventory other)
    {
        foreach ((var snackType, var quantity) in Snacks)
        {
            if (!other.Snacks.ContainsKey(snackType))
            {
                other.Snacks[snackType] = quantity;
                continue;
            }

            other.Snacks[snackType] += quantity;
        }
    }
}

public struct Item
{
    public ItemType Type;
    public SnackType? Snack;
    public CharmType? CharmType;

    public static Item NewSnack(SnackType snackType) => new()
    {
        Type = ItemType.Snack,
        Snack = snackType,
    };

    public static Item NewCharm(CharmType charmType) => new()
    {
        Type = ItemType.Charm,
        CharmType = charmType,
    };
}

public enum ItemType
{
    Snack,
    Charm,
}

public struct Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Tile
    {
        public ushort Type;
        public byte HeightBits;
        public byte ExtraBits;
    }

    public required Tile[,] Walls;
    public required Tile[,] Things;
    public required Tile[,] Extras;
}

public enum MusicTrack
{
    Battle,
    Victory,
    WanderingStage1,
    WanderingStage2,
}

public struct Player
{
    public struct Defaults()
    {
        public double WalkCooldown = 1 / 4.0;
        public float Health = 100;
    }

    public Defaults Default = new();
    public Defaults Current = new();
    public Inventory Inventory = new();
    public float RunningHealth;

    public Player()
    {
        RunningHealth = Default.Health;
    }
}

public struct RenderThing
{
    public Matrix4x4 Transform;
    public ShapeType ShapeType;
    public int SubFrame;
    public float CurrentAnimationTime;
    public bool IsVisible;
}

public interface IRenderable
{
    RenderThing GetRenderThing();
}

public enum ShapeType
{
    Placeholder,
    Chest,
    Turret,
    Goon,
}

public enum SnackType
{
    ChickenLeg,
    WholeChicken,
}

public enum SoundEffect
{
    BattleStart,
    Clap,
    Crit,
    Die,
    Flame,
    Hough,
    HPUp,
    Item,
    MachineGun,
    Miss,
    OpenChest,
    Shuffle,
    Smack,
    Step,
    Talk,
    UICancel,
    UIConfirm,
    UIFocus,
}

public class TimeContext
{
    public double Delta;
    public double Time;
}

public struct Transform
{
    public Position Position;
    public int Direction;
}

public interface IAudioService
{
    void PlaySoundEffect(SoundEffect soundEffect);
    void ChangeMusic(MusicTrack musicTrack);
    void StopMusic();
}

public interface IInputService
{
    bool ActionWasJustPressed(InputAction action);
    bool ActionWasJustReleased(InputAction action);
    bool ActionIsPressed(InputAction action);
}

public enum InputAction
{
    MoveForward,
    MoveRight,
    MoveBack,
    MoveLeft,
    LookRight,
    LookLeft,
    Confirm,
    Cancel,

    //
    // [TODO]: temporary
    //
    BattleAttack,
    BattleDefend,
    BattleRun,
    BattleItem,
    DebugBattleScreen,
}

public interface IResettable
{
    public void Reset();
}

public interface IResettable<TArg>
{
    public void Reset(TArg arg);
}

public enum FoeStateFlag
{
    Idle,
    BeginAttacking,
    Attacking,
    Hurt,
    Dying,
    FlyingOffscreen,
}

public enum FoeType
{
    Turret,
    Goon,
}

public class GameContext
{
    public required IAudioService AudioService;
    public required IInputService InputService;
    public required TimeContext TimeContext;
}

public struct Position
{
    public int X;
    public int Y;

    public Position() {}

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public interface IProgressTracker
{
    double GetProgress();
}

public interface IFoe
{
    void DoAttackSequence();
}
