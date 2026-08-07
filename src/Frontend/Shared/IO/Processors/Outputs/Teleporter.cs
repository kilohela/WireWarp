using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class Processor
{
    private static void Teleporter(WiringGraph graph, Output output)
    {
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            var wire = op.Fanin.OfType<Wire>().First();

            if (graph.WiringExtra.Teleporter.Keys.Any(k =>
                    k.Source == op.Source &&
                    k.Fanin.OfType<Wire>().First() == wire))
                goto remove;

            var wireMap = new Dictionary<((int, int), WireID), Wire>();
            var founds = Conversion.TraceWires.TraceWire(
                wire, op.Source, op.Source, graph, wireMap);

            var teleporters = founds
                .Where(f => f.component is Output o && o.Type == OutputID.Teleporter)
                .ToList();

            if (teleporters.Count < 2) goto remove;

            var source = teleporters[0];
            var target = teleporters[^1];

            if (source == target) goto remove;

            graph.WiringExtra.Teleporter[op] = (source.active, target.active);
            continue;

        remove:
            graph.RemoveNode(op);
        }
    }
}
