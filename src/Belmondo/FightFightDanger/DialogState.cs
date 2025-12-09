using System.Text;

namespace Belmondo.FightFightDanger;

public struct DialogState()
{
    public string? TargetLine;
    public StringBuilder RunningLine = new();
    public float CurrentCharacterIndex;
}
