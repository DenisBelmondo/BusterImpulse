namespace Belmondo.FightFightDanger;

public static class GoonFoe
{
    public static State CreateAttackState()
    {
        return new()
        {
            UpdateFunction = () =>
            {
                return State.Continue;
            },
        };
    }
}
