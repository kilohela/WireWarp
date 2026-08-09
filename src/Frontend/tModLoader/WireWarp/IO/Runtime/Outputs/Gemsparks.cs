using Terraria;
using Terraria.ID;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Gemsparks(IOGraph iOGraph, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        int type = tile.TileType;
        if (type >= TileID.AmethystGemsparkOff && type <= TileID.AmberGemspark)
        {
            if (!tile.HasActuator)
            {
                if (type >= TileID.AmethystGemspark)
                    tile.TileType = (ushort)(type - 7);
                else
                    tile.TileType = (ushort)(type + 7);

                WorldGen.SquareTileFrame(i, j);
                NetMessage.SendTileSquare(-1, i, j);
            }
        }
    }
}
