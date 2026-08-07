using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.IO;

partial class Processor
{
    private static void WireBulb(WiringGraph graph, Output output)
    {
        foreach (var op in output.Fanin.OfType<OutputPort>())
        {
            var wire = op.Fanin.OfType<Wire>().First();
            graph.WiringExtra.WireBulb[op] = wire.Type;
        }
    }
}
