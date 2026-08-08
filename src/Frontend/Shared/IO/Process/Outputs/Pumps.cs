using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.Shared.Terraria.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void Pumps(WiringGraph graph, Output output)
    {
        var op = output.Fanin.OfType<OutputPort>().First();

        var seen = new HashSet<(int X, int Y)>();
        foreach (var wire in op.Fanin.OfType<Wire>())
        foreach (var sourcePos in wire.Sources)
        {
            if (!seen.Add(sourcePos)) continue;

            var wireMap = new Dictionary<((int, int), WireID), Wire>();
            var founds = Conversion.TraceWires.TraceWire(
                wire, sourcePos, sourcePos, graph, wireMap);

            var pumps = founds
                .Where(f => f.component is Output { Type: OutputID.Pumps })
                .ToList();

            var inlets = new List<(int x, int y)>();
            var outlets = new List<(int x, int y)>();
            
            var visited = new HashSet<Output>();
            foreach (var (active, component) in pumps)
            {
                var pump = (Output)component;
                if (!visited.Add(pump)) continue;

                var tileType = Main.tile[active.x, active.y].type;
                if (tileType == TileID.InletPump)
                    inlets.Add(active);
                else if (tileType == TileID.OutletPump)
                    outlets.Add(active);
            }

            if (inlets.Count == 0 || outlets.Count == 0 || 
                graph.OutputPos[inlets[0]] != output) continue;

            var source = graph.GatePos.TryGetValue(sourcePos, out Gate? gate)
                ? (IConnectable)gate
                : graph.InputPos[sourcePos].Fanout.OfType<InputPort>().First();

            var newWire = graph.AddWire(wire.Type);
            var newOp = graph.AddOutputPort();

            WiringGraph.AddEdge(source, newWire);
            WiringGraph.AddEdge(newWire, newOp);
            WiringGraph.AddEdge(newOp, output);

            newWire.Sources.Add(sourcePos);
            newWire.Drains.UnionWith(pumps.Select(p => p.active));

            graph.WiringExtra.Pumps[newOp] = (inlets, outlets);
        }

        graph.RemoveNode(op);
    }
}
