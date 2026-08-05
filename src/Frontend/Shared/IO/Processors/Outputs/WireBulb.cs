using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.IO;

partial class Processor
{
    static void WireBulb(WiringGraph graph, Output output)
    {
        foreach (var op in output.Fanin.OfType<OutputPort>())
        {
            var wire = op.Fanin.OfType<Wire>().First();
            graph.ExtraData.WireBulb[op] = wire.Type;
        }
    }
}
