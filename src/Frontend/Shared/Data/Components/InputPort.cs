namespace WireWarp.Frontend.Shared.Data;

public class InputPort : IConnectable
{
    public int Id { get; init; }
    public int PortId { get; set; }

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
