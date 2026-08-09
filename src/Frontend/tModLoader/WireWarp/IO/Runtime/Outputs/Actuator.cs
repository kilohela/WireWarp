using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Actuator(IOGraph iOGraph, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (tile.HasActuator)
            Wiring.ActuateForced(i, j);
    }
}
