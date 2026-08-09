using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeInput
{
    private static void GemLocks(IOGraph iOGraph, int i, int j) =>
        PressurePlates(iOGraph, i + 1, j + 1);
}
