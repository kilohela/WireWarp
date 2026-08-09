using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void OpenDoors(IOGraph iOGraph, int i, int j)
    {
        if (WorldGen.CloseDoor(i, j, forced: true))
            NetMessage.SendData(19, -1, -1, null, 1, i, j);
    }
}
