using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class InputPort : IConnectable
{
    public int Id { get; init; }
    public int PortId { get; set; }

    InputID Type => Fanin.OfType<Input>().FirstOrDefault() 
        is Input i ? i.Type : InputID.None;
    byte IConnectable.Type => (byte)Type;

    public HashSet<IConnectable> Fanin { get; } = [];
    public HashSet<IConnectable> Fanout { get; } = [];
}
