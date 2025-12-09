namespace Belmondo.FightFightDanger;

public enum ChestStatus
{
    Idle,
    Opening,
    Opened,
}

public struct ChestState
{
    public float Openness;
    public ChestStatus Status;
}
