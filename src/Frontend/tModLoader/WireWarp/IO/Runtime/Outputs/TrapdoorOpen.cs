using Terraria;
using Terraria.ID;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void TrapdoorOpen(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        bool value = tile.TileType == TileID.TrapdoorClosed;
        int num66 = WorldGen.ShiftTrapdoor(i, j, playerAbove: true).ToInt();
        if (num66 == 0)
            num66 = -WorldGen.ShiftTrapdoor(i, j, playerAbove: false).ToInt();

        if (num66 != 0)
            NetMessage.SendData(19, -1, -1, null, 3 - value.ToInt(), i, j, num66);
    }
}
