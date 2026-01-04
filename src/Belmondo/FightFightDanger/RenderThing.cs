using System.Numerics;

namespace Belmondo.FightFightDanger;

public struct RenderThing
{
    public Matrix4x4 Transform;
    public ShapeType ShapeType;
    public int SubFrame;
    public float CurrentAnimationTime;
    public bool IsVisible;
}
