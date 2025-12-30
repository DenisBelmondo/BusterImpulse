namespace Belmondo.FightFightDanger;

public interface IResettable
{
    public void Reset();
}

public interface IResettable<TArg>
{
    public void Reset(TArg arg);
}
