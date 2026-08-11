using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Hashcode
{
    public static void Execute()
    {
        WiringGraph.SetHash(WiringHash.GetHash());
    }
}
