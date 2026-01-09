using System.Runtime.InteropServices;

namespace Belmondo.FightFightDanger;

public struct Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Tile
    {
        public ushort Type;
        public byte HeightBits;
        public byte ExtraBits;
    }

    public required Tile[,] Walls;
    public required Tile[,] Things;
    public required Tile[,] Extras;
}
