using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Lights(IOGraph iOGraph, int i, int j)
        => Wiring.Toggle2x2Light(i, j, Main.tile[i, j], null, false);
}
