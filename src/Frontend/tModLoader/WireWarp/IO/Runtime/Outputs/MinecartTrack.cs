using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void MinecartTrack(IOGraph iOGraph, int i, int j)
    {
        if (iOGraph.IOTemp.CheckMech(i, j, 5))
        {
            Minecart.FlipSwitchTrack(i, j);
        }
    }
}
