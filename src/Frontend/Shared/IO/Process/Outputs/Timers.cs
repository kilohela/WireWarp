using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void Timers(WiringGraph graph, Output output)
    {
        // Timer output cannot directly activate itself.
        foreach (var op in output.Fanin.OfType<OutputPort>())
        foreach (var wire in op.Fanin.OfType<Wire>().ToList())
        {
            if (wire.Fanin.OfType<InputPort>()
                .Any(ip => ip.Fanin.OfType<Input>()
                .Any(input => input.Origin == output.Origin)))
            {
                WiringGraph.RemoveEdge(wire, op);
            }
        }
    }
}
