using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void ClosedDoors(IOGraph iOGraph, int i, int j)
    {
        int num67 = 1;
        if (Main.rand.Next(2) == 0)
            num67 = -1;

        if (!WorldGen.OpenDoor(i, j, num67))
        {
            if (WorldGen.OpenDoor(i, j, -num67))
                NetMessage.SendData(19, -1, -1, null, 0, i, j, -num67);
        }
        else
        {
            NetMessage.SendData(19, -1, -1, null, 0, i, j, num67);
        }
    }
}
