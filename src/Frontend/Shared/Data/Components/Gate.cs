using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class Gate : IConnectable
{
    public int Id { get; init; }
    
    public GateID Type { get; init; }
    public (int X, int Y) Origin { get; init; }

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
