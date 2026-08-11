using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class OutputPort : IConnectable
{
    public int Id { get; init; }
    public int PortId { get; set; }

    OutputID Type => Fanout.OfType<Output>().FirstOrDefault()
        is Output o ? o.Type : OutputID.None;
    byte IConnectable.Type => (byte)Type;

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
