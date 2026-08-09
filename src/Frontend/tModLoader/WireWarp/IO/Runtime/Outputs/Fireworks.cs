using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Fireworks(IOGraph iOGraph, int i, int j)
    {
        WorldGen.LaunchRocket(i, j, fromWiring: true);
        // SkipWire(i, j);
    }
}
