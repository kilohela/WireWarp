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

public class IOTemp
{
    private readonly Dictionary<(int x, int y), int> _mechTime = [];

    public int cannonCoolDown = 0;
    public int bunnyCannonCoolDown = 0;
    public int snowballCannonCoolDown = 0;

    public bool CheckMech(int i, int j, int time)
    {
        if (time <= 0)
            return true;

        if (_mechTime.TryGetValue((i, j), out var remaining) && remaining > 0)
            return false;

        _mechTime[(i, j)] = time;
        return true;
    }

    public void UpdateMech()
    {
        if (cannonCoolDown > 0)
            cannonCoolDown--;

        if (bunnyCannonCoolDown > 0)
            bunnyCannonCoolDown--;

        if (snowballCannonCoolDown > 0)
            snowballCannonCoolDown--;

        if (_mechTime.Count == 0) return;

        var expired = new List<(int x, int y)>();
        foreach (var (key, value) in _mechTime)
        {
            var remaining = value - 1;
            if (remaining <= 0)
                expired.Add(key);
            else
                _mechTime[key] = remaining;
        }

        expired.ForEach(e => _mechTime.Remove(e));
    }

    public void Reset()
    {
        cannonCoolDown = 0;
        bunnyCannonCoolDown = 0;
        snowballCannonCoolDown = 0;
        _mechTime.Clear();
    }
}

public class IOGraph
{
    public IOExtra IOExtra { get; init; }
    public IOTemp IOTemp { get; } = new();

    private readonly Dictionary<(int x, int y), (int portId, InputID type)> _inputs = [];
    private readonly Dictionary<int, ((int x, int y), OutputID type)> _outputs = [];

    public IReadOnlyDictionary<(int x, int y), (int portId, InputID type)> Inputs => _inputs;
    public IReadOnlyDictionary<int, ((int x, int y), OutputID type)> Outputs => _outputs;

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
            var wire = op.Fanin.OfType<Wire>().First();
            var pos = wire.Drains.First(d => graph.OutputPos[d] == output);
            _outputs[op.PortId] = (pos, output.Type);
        }

        IOExtra = new IOExtra(graph.WiringExtra);
    }
}
