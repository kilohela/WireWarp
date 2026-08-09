using Terraria;
using Terraria.ID;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Torches(IOGraph iOGraph, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (TileID.Sets.Torches[tile.TileType])
            Wiring.ToggleTorch(i, j, tile, null);
    }
}
