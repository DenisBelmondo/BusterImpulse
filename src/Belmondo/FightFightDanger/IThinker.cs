namespace Belmondo.FightFightDanger;

public interface IThinker
{
    public void Update();
}

public interface IThinker<TArg>
{
    public void Update(TArg arg);
}
