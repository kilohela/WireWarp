using Terraria.ModLoader;
using WireWarp.Frontend.Shared;

namespace WireWarp.Frontend.tModLoader;

internal sealed class WireWarpCommand : ModCommand
{
    public override CommandType Type => CommandType.World;
    public override string Command => "ww";
    public override string Usage => "/ww <startup|shutdown|run|stop|syncto|syncfrom|reset|report>";
    public override string Description => "WireWarp control commands";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0)
        {
            caller.Reply(Usage);
            return;
        }

        try
        {
            switch (args[0])
            {
                case "startup":
                    Runtime.Startup();
                    caller.Reply("WireWarp started");
                    break;

                case "shutdown":
                    Runtime.Shutdown();
                    caller.Reply("WireWarp shutdown");
                    break;

                case "run":
                    Runtime.Run();
                    caller.Reply("WireWarp running");
                    break;

                case "stop":
                    Runtime.Stop();
                    caller.Reply("WireWarp stopped");
                    break;

                case "syncto":
                    Runtime.SyncTo();
                    caller.Reply("WireWarp synced to backend");
                    break;

                case "syncfrom":
                    Runtime.SyncFrom();
                    caller.Reply("WireWarp synced from backend");
                    break;

                case "reset":
                    Runtime.Reset();
                    caller.Reply("WireWarp reset");
                    break;

                case "report":
                    Runtime.Report();
                    caller.Reply("WireWarp report written");
                    break;

                default:
                    caller.Reply(Usage);
                    break;
            }
        }
        catch (Exception e)
        {
            caller.Reply($"WireWarp command failed: {e.Message}");
            ModContent.GetInstance<WireWarp>().Logger.Error($"WireWarp command failed: {e}");
        }
    }
}
