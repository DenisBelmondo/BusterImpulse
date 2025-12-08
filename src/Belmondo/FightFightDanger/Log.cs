namespace Belmondo.FightFightDanger;

public class Log(int maxLogLines)
{
    public List<string> Lines { get; private set; } = [];

    public void Add(string line)
    {
        Lines.Add(line);

        if (Lines.Count > maxLogLines)
        {
            Lines.RemoveAt(0);
        }
    }

    public void Clear()
    {
        Lines.Clear();
    }
}
