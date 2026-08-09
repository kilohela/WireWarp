using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Chandeliers(IOGraph iOGraph, int i, int j)
        => Wiring.ToggleChandelier(i, j, Main.tile[i, j], null, false);
}
