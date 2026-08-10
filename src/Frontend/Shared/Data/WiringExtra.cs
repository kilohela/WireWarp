using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class WiringExtra
{
    public Dictionary<OutputPort, ((int x, int y) source, (int x, int y) target)> Teleporter { get; } = [];
    public Dictionary<OutputPort, (List<(int x, int y)> inlets, List<(int x, int y)> outlets)> Pumps { get; } = [];
    public Dictionary<OutputPort, WireID> WireBulb { get; } = [];
}
