using Terraria;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void LandMine(int i, int j)
        => Wiring.ExplodeMine(i, j);
}
