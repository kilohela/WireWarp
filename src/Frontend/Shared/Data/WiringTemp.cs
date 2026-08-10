using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class WiringTemp
{
    public Dictionary<((int X, int Y) Pos, WireID Type), List<((int x, int y) active, IConnectable component)>> Traces { get; } = [];
}
