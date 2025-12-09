namespace Belmondo.FightFightDanger;

public struct GameState()
{
    public WorldState WorldState = new();
    public BattleState BattleState;
    public DialogState DialogState;
}
