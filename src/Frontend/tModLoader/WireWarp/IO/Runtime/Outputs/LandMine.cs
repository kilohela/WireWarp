using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void LandMine(IOGraph iOGraph, int i, int j)
        => Wiring.ExplodeMine(i, j);
}
