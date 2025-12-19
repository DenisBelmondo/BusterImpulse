using System.Numerics;

namespace Belmondo.FightFightDanger;

public struct RenderThing
{
    public Matrix4x4 Transform;
    public int AnimationID;
    public float CurrentAnimationTime;
    public bool IsVisible;
}
