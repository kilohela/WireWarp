using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.Shared.Terraria.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void Pumps(WiringGraph graph, Output output)
    {
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            var wire = op.Fanin.OfType<Wire>().First();

            if (graph.WiringExtra.Pumps.Keys.Any(k =>
                    k.Source == op.Source &&
                    k.Fanin.OfType<Wire>().First() == wire))
                goto remove;

            var wireMap = new Dictionary<((int, int), WireID), Wire>();
            var founds = Conversion.TraceWires.TraceWire(
                wire, op.Source, op.Source, graph, wireMap);

            var inlets = new List<(int x, int y)>();
            var outlets = new List<(int x, int y)>();

            var visited = new HashSet<Output>();
            foreach (var (active, component) in founds.Where(f =>
                f.component is Output { Type: OutputID.Pumps }))
            {
                var pump = (Output)component;
                if (!visited.Add(pump)) continue;

                var tileType = Main.tile[active.x, active.y].type;
                if (tileType == TileID.InletPump)
                    inlets.Add(active);
                else if (tileType == TileID.OutletPump)
                    outlets.Add(active);
            }

            if (inlets.Count == 0 || outlets.Count == 0) goto remove;

            graph.WiringExtra.Pumps[op] = (inlets, outlets);
            continue;

        remove:
            graph.RemoveNode(op);
        }
    }
}
