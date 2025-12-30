namespace Belmondo.FightFightDanger;

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
