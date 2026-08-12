using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class Output : IConnectable
{
    public int Id { get; init; }
    
    public OutputID Type { get; init; }
    byte IConnectable.Type => (byte)Type;
    
    public (int X, int Y) Origin { get; set; }

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
