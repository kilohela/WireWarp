using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.Shared.Terraria.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class Processor
{
    private static void Pumps(WiringGraph graph, Output output)
    {
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            var wire = op.Fanin.OfType<Wire>().First();

            if (graph.ExtraData.Pumps.Keys.Any(k =>
                    k.X == op.X && k.Y == op.Y &&
                    k.Fanin.OfType<Wire>().First() == wire))
                goto remove;

            var wireMap = new Dictionary<((int, int), WireID), Wire>();
            var found = Conversion.TraceWires.TraceWire(
                wire, (op.X, op.Y), (op.X, op.Y), graph, wireMap);

            var inlets = new List<Output>();
            var outlets = new List<Output>();

            foreach (var pump in found.OfType<Output>().Where(o => o.Type == OutputID.Pumps))
            {
                var tileType = Main.tile[pump.X, pump.Y].type;
                if (tileType == TileID.InletPump && !inlets.Contains(pump))
                    inlets.Add(pump);
                else if (tileType == TileID.OutletPump && !outlets.Contains(pump))
                    outlets.Add(pump);
            }

            if (inlets.Count == 0 || outlets.Count == 0) goto remove;

            graph.ExtraData.Pumps[op] = (inlets, outlets);
            continue;

        remove:
            graph.RemoveNode(op);
        }
    }
}
