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

            if (graph.ExtraData.Teleporter.Keys.Any(k =>
                    k.X == op.X && k.Y == op.Y &&
                    k.Fanin.OfType<Wire>().First() == wire))
                goto remove;

            var wireMap = new Dictionary<((int, int), WireID), Wire>();
            var found = Conversion.TraceWires.TraceWire(
                wire, (op.X, op.Y), (op.X, op.Y), graph, wireMap);

            var teleporters = found
                .OfType<Output>()
                .Where(o => o.Type == OutputID.Teleporter)
                .ToList();

            if (teleporters.Count < 2) goto remove;

            var source = teleporters[0];
            var target = teleporters[^1];

            if (source == target) goto remove;

            graph.ExtraData.Teleporter[op] = (source, target);
            continue;

        remove:
            graph.RemoveNode(op);
        }
    }
}
