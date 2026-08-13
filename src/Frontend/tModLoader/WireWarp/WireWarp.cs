using System;
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
        try { Runtime.HitInput(i, j); }
        catch (Exception e) 
        { ModContent.GetInstance<WireWarp>().Logger.Error($"WireWarp hit switch failed: {e}"); }
    }
}

internal sealed class WireWarpSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        try { Runtime.Startup(); }
        catch (Exception e)
        { ModContent.GetInstance<WireWarp>().Logger.Error($"WireWarp startup failed: {e}"); }
    }

    public override void PostUpdateWorld()
    {
        try
        {
            Runtime.Tick();
        }
        catch (Exception e)
        {
            ModContent.GetInstance<WireWarp>().Logger.Error($"WireWarp tick failed: {e}");
            try { Runtime.Shutdown(); } catch { }
        }
    }

    public override void OnWorldUnload()
    {
        try { Runtime.Shutdown(); }
        catch (Exception e)
        { ModContent.GetInstance<WireWarp>().Logger.Error($"WireWarp shutdown failed: {e}"); }
    }
}
