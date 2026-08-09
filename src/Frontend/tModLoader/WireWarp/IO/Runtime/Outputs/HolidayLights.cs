using Terraria;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void HolidayLights(IOGraph iOGraph, int i, int j)
        => Wiring.ToggleHolidayLight(i, j, Main.tile[i, j], null);
}
