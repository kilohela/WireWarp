using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void HangingLanterns(IOGraph iOGraph, int i, int j)
        => Wiring.ToggleHangingLantern(i, j, Main.tile[i, j], null, false);
}
