using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void WireBulb(WiringGraph graph, Output output)
    {
        var oldOp = output.Fanin.OfType<OutputPort>().First();
        
        foreach (var wire in oldOp.Fanin.OfType<Wire>().ToList())
        {
            var newOp = graph.AddOutputPort();
            WiringGraph.AddEdge(wire, newOp);
            WiringGraph.AddEdge(newOp, output);
        }

        graph.RemoveNode(oldOp);

        foreach (var op in output.Fanin.OfType<OutputPort>())
        {
            var wire = op.Fanin.OfType<Wire>().First();
            graph.WiringExtra.WireBulb[op] = wire.Type;
        }
    }
}
