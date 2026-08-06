using Terraria.ModLoader;
using WireWarp.Frontend.Shared.Conversion;

namespace WireWarp.Frontend.tModLoader;

public sealed class WireWarp : Mod
{
}

internal sealed class WireWarpSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        Converter.Convert();
    }
}
