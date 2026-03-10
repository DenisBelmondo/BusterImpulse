namespace Belmondo.FightFightDanger;

public class Goon : IFoe
{
    public enum State
    {
        Idle,
        BeginAttacking,
        Attacking,
        Hurt,
        Dying,
    }

    public void DoAttackSequence()
    {
    }
}
