using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class IOExtra
{
    private readonly Dictionary<int, ((int x, int y) source, (int x, int y) target)> _teleporter = [];
    private readonly Dictionary<int, (List<(int x, int y)> inlets, List<(int x, int y)> outlets)> _pumps = [];
    private readonly Dictionary<int, WireID> _wireBulb = [];

    public IReadOnlyDictionary<int, ((int x, int y) source, (int x, int y) target)> Teleporter => _teleporter;
    public IReadOnlyDictionary<int, (List<(int x, int y)> inlets, List<(int x, int y)> outlets)> Pumps => _pumps;
    public IReadOnlyDictionary<int, WireID> WireBulb => _wireBulb;

    public IOExtra(WiringExtra source)
    {
        foreach (var (op, v) in source.Teleporter) _teleporter[op.PortId] = v;
        foreach (var (op, v) in source.Pumps) _pumps[op.PortId] = v;
        foreach (var (op, v) in source.WireBulb) _wireBulb[op.PortId] = v;
    }
}

public class IOGraph
{
    private readonly Dictionary<(int X, int Y), (int PortId, InputID Type)> _inputs = [];
    private readonly Dictionary<int, (int X, int Y, OutputID Type)> _outputs = [];

    public IReadOnlyDictionary<(int X, int Y), (int PortId, InputID Type)> Inputs => _inputs;
    public IReadOnlyDictionary<int, (int X, int Y, OutputID Type)> Outputs => _outputs;
    public IOExtra IOExtra { get; init; }

    public IOGraph(WiringGraph graph)
    {
        foreach (var (pos, input) in graph.InputPos)
        {
            var ip = input.Fanout.OfType<InputPort>().FirstOrDefault();
            if (ip != null) _inputs[pos] = (ip.PortId, input.Type);
        }

        foreach (var op in graph.OutputPorts)
        {
            var output = op.Fanout.OfType<Output>().First();
            _outputs[op.PortId] = (op.Drain.X, op.Drain.Y, output.Type);
        }

        IOExtra = new IOExtra(graph.WiringExtra);
    }
}
