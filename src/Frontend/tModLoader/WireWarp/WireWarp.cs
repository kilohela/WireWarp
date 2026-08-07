using Terraria.ModLoader;
using WireWarp.Frontend.Shared;
using WireWarp.Frontend.Shared.Conversion;

namespace WireWarp.Frontend.tModLoader;

internal sealed class WireWarp : Mod
{
    public override void Load()
    {
        Access.Instance = new Accessor();
    }
}

internal sealed class WireWarpSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        Converter.Execute();
    }
}
