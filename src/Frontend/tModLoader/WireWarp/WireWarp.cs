using Terraria;
using Terraria.ModLoader;
using WireWarp.Frontend.Shared;

namespace WireWarp.Frontend.tModLoader;

internal sealed class WireWarp : Mod
{
    public override void Load()
    {
        Access.Instance = new Accessor();
        On_Wiring.HitSwitch += OnHitSwitch;
    }

    public override void Unload()
    {
        On_Wiring.HitSwitch -= OnHitSwitch;
    }

    private static void OnHitSwitch(On_Wiring.orig_HitSwitch orig, int i, int j)
    {
        Runtime.HitInput(i, j);
    }
}

internal sealed class WireWarpSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        Runtime.Startup();
        Runtime.SyncTo();
    }

    public override void PostUpdateWorld()
    {
        Runtime.Tick();
    }

    public override void OnWorldUnload()
    {
        Runtime.Shutdown();
    }
}
