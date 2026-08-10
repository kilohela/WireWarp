using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void Timers(Output output)
    {
        // Timer output cannot directly activate itself.
        var op = output.Fanin.OfType<OutputPort>().First();
        
        foreach (var wire in op.Fanin.OfType<Wire>().ToList())
        {
            if (wire.Fanin.OfType<InputPort>()
                .Any(ip => ip.Fanin.OfType<Input>()
                .Any(input => input.Origin == output.Origin && 
                    input.Type == InputID.Timers)))
            {
                WiringGraph.RemoveEdge(wire, op);
            }
        }
    }
}
