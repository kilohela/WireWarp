using Terraria;
using Terraria.ID;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Campfires(IOGraph iOGraph, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (TileID.Sets.Campfires[tile.TileType])
            Wiring.ToggleCampFire(i, j, tile, null, false);
    }
}
