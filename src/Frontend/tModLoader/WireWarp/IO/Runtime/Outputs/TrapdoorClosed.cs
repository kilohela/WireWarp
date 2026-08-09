using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void TrapdoorClosed(IOGraph iOGraph, int i, int j)
        => TrapdoorOpen(iOGraph, i, j);
}
