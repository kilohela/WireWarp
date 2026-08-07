namespace WireWarp.Frontend.Shared.Data;

public class OutputPort : IConnectable
{
    public int Id { get; init; }
    public int PortId { get; set; }
    public (int X, int Y) Source { get; init; }
    public (int X, int Y) Drain { get; init; }

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
