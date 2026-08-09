using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Candles(IOGraph iOGraph, int i, int j)
        => Wiring.ToggleCandle(i, j, Main.tile[i, j], null);
}
