namespace Belmondo.FightFightDanger;

public class GameContext
{
    public required IAudioService AudioService;
    public required IInputService InputService;
    public double Delta;
    public double Time;
}
