using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void Teleporter(WiringGraph graph, Output output)
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

            var teleporters = founds
                .Where(f => f.component is Output { Type: OutputID.Teleporter })
                .ToList();

            var origin = teleporters[0];
            var target = teleporters[^1];

            if (origin == target || origin.component != output) continue;

            var source = graph.GatePos.TryGetValue(sourcePos, out Gate? gate)
                ? (IConnectable)gate
                : graph.InputPos[sourcePos].Fanout.OfType<InputPort>().First();

            var newWire = graph.AddWire(wire.Type);
            var newOp = graph.AddOutputPort();

            WiringGraph.AddEdge(source, newWire);
            WiringGraph.AddEdge(newWire, newOp);
            WiringGraph.AddEdge(newOp, output);

            newWire.Sources.Add(sourcePos);
            newWire.Drains.UnionWith([origin.active, target.active]);

            graph.WiringExtra.Teleporter[newOp] = (origin.active, target.active);
        }

        graph.RemoveNode(op);
    }
}
