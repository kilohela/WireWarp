using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Timers(IOGraph iOGraph, int i, int j)
    {
        RuntimeInput.Timers(iOGraph, i, j);
        WorldGen.SquareTileFrame(i, j);
        NetMessage.SendTileSquare(-1, i, j);
    }
}
