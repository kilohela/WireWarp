using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class OutputPort : IConnectable
{
    public int Id { get; set; }
    public int PortId => Id - WiringGraph.OutputPortOffset;

    OutputID Type => Fanout.OfType<Output>().FirstOrDefault()
        is Output o ? o.Type : OutputID.None;
    byte IConnectable.Type => (byte)Type;

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
