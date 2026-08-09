using Terraria;
using Terraria.ID;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void ConveyorBelts(IOGraph iOGraph, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (!tile.HasActuator)
        {
            if (tile.TileType == TileID.ConveyorBeltLeft)
                tile.TileType = TileID.ConveyorBeltRight;
            else
                tile.TileType = TileID.ConveyorBeltLeft;

            WorldGen.SquareTileFrame(i, j);
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
}
