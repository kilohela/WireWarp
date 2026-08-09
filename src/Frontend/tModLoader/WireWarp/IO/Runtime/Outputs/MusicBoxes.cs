using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void MusicBoxes(IOGraph iOGraph, int i, int j)
        => WorldGen.SwitchMB(i, j);
}
