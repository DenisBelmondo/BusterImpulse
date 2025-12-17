namespace Belmondo.FightFightDanger;

public class Animation<TKeyframe>
{
    public readonly Dictionary<double, List<TKeyframe>> Keyframes = [];

    public void AddKeyframe(double timestampSeconds, TKeyframe value)
    {
        if (!Keyframes.ContainsKey(timestampSeconds))
        {
            Keyframes.Add(timestampSeconds, []);
        }

        Keyframes[timestampSeconds].Add(value);
    }
}
