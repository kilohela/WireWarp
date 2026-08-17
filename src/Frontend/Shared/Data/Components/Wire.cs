using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class Wire : IConnectable
{
    public int Id { get; set; }
    
    public WireID Type { get; init; }
    byte IConnectable.Type => (byte)Type;

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];

    public HashSet<(int X, int Y)> Sources { get; } = [];
    public HashSet<(int X, int Y)> Drains { get; } = [];
}
