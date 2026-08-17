using WireWarp.Frontend.Shared.Conversion;

namespace WireWarp.Frontend.Shared.Data;

public static class WiringGraph
{
    private static readonly byte[] _hash = new byte[32];

    private static readonly HashSet<Wire> _wires = [];
    private static readonly HashSet<Gate> _gates = [];
    private static readonly HashSet<Lamp> _lamps = [];
    private static readonly HashSet<Input> _inputs = [];
    private static readonly HashSet<InputPort> _inputPorts = [];
    private static readonly HashSet<Output> _outputs = [];
    private static readonly HashSet<OutputPort> _outputPorts = [];

    public static ReadOnlyMemory<byte> Hash => _hash;

    public static int InputPortOffset => 0;
    public static int OutputPortOffset => InputPortOffset + _inputPorts.Count;
    public static int LampOffset => OutputPortOffset + _outputPorts.Count;
    public static int GateOffset => LampOffset + _lamps.Count;
    public static int WireOffset => GateOffset + _gates.Count;
    public static int InputOffset => WireOffset + _wires.Count;
    public static int OutputOffset => InputOffset + _inputs.Count;

    public static IReadOnlySet<Wire> Wires => _wires;
    public static IReadOnlySet<Gate> Gates => _gates;
    public static IReadOnlySet<Lamp> Lamps => _lamps;
    public static IReadOnlySet<Input> Inputs => _inputs;
    public static IReadOnlySet<InputPort> InputPorts => _inputPorts;
    public static IReadOnlySet<Output> Outputs => _outputs;
    public static IReadOnlySet<OutputPort> OutputPorts => _outputPorts;

    public static Dictionary<int, IConnectable> Components { get; } = [];

    internal static Dictionary<(int x, int y), Gate> GatePos { get; } = [];
    internal static Dictionary<(int x, int y), Lamp> LampPos { get; } = [];
    internal static Dictionary<(int x, int y), Input> InputPos { get; } = [];
    internal static Dictionary<(int x, int y), Output> OutputPos { get; } = [];

    internal static void SetHash(byte[] hash) => hash.CopyTo(_hash, 0);

    // edge

    internal static void AddEdge(IConnectable from, IConnectable to)
    {
        from.Fanout.Add(to);
        to.Fanin.Add(from);
    }

    internal static void RemoveEdge(IConnectable from, IConnectable to)
    {
        from.Fanout.Remove(to);
        to.Fanin.Remove(from);
    }

    internal static T AddNode<T>(T node) where T : IConnectable
    {
        switch (node)
        {
            case Wire w: _wires.Add(w); break;
            case Gate g: _gates.Add(g); break;
            case Lamp l: _lamps.Add(l); break;
            case Input i: _inputs.Add(i); break;
            case Output o: _outputs.Add(o); break;
            case InputPort ip: _inputPorts.Add(ip); break;
            case OutputPort op: _outputPorts.Add(op); break;
        }
        return node;
    }

    internal static void RemoveNode(IConnectable node)
    {
        foreach (var source in node.Fanin)
            source.Fanout.Remove(node);

        foreach (var target in node.Fanout)
            target.Fanin.Remove(node);

        node.Fanin.Clear();
        node.Fanout.Clear();

        switch (node)
        {
            case Wire w: _wires.Remove(w); break;
            case Gate g: _gates.Remove(g); break;
            case Lamp l: _lamps.Remove(l); break;
            case Input i: _inputs.Remove(i); break;
            case Output o: _outputs.Remove(o); break;
            case InputPort ip: _inputPorts.Remove(ip); break;
            case OutputPort op: _outputPorts.Remove(op); break;
        }
    }

    public static void Build()
    {
        Clean();

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // preprocess
        Scan.Execute();
        Access.Instance.Status($"Scan time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Trace.Execute();
        Access.Instance.Status($"Trace time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();

        // postprocess
        Prune.Execute();
        Access.Instance.Status($"Prune time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Normalize.Execute();
        Access.Instance.Status($"Normalize time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Prune.Execute();
        Access.Instance.Status($"Prune time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Applier.Execute();
        Access.Instance.Status($"Applier time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Prune.Execute();
        Access.Instance.Status($"Prune time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Assign.Execute();
        Access.Instance.Status($"Assign time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        Validate.Execute();
        Access.Instance.Status($"Validate time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
    }

    public static void Resolve()
    {
        Apply.Execute();
    }

    public static void Clean()
    {
        _wires.Clear();
        _gates.Clear();
        _lamps.Clear();
        _inputs.Clear();
        _inputPorts.Clear();
        _outputs.Clear();
        _outputPorts.Clear();

        Components.Clear();
        GatePos.Clear();
        LampPos.Clear();
        InputPos.Clear();
        OutputPos.Clear();

        Array.Clear(_hash);

        WiringExtra.Clean();
        WiringTemp.Clean();
    }
}
