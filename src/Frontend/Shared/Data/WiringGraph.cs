using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Conversion;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared.Data;

public static class WiringGraph
{
    private static readonly byte[] _hash = new byte[32];

    private static readonly Dictionary<int, IConnectable> _components = [];
    private static int _nextComponentId;

    private static readonly HashSet<Wire> _wires = [];
    private static readonly HashSet<Gate> _gates = [];
    private static readonly HashSet<Lamp> _lamps = [];
    private static readonly HashSet<Input> _inputs = [];
    private static readonly HashSet<InputPort> _inputPorts = [];
    private static readonly HashSet<Output> _outputs = [];
    private static readonly HashSet<OutputPort> _outputPorts = [];

    public static ReadOnlyMemory<byte> Hash => _hash;

    public static IReadOnlyDictionary<int, IConnectable> Components => _components;

    public static IReadOnlySet<Wire> Wires => _wires;
    public static IReadOnlySet<Gate> Gates => _gates;
    public static IReadOnlySet<Lamp> Lamps => _lamps;
    public static IReadOnlySet<Input> Inputs => _inputs;
    public static IReadOnlySet<InputPort> InputPorts => _inputPorts;
    public static IReadOnlySet<Output> Outputs => _outputs;
    public static IReadOnlySet<OutputPort> OutputPorts => _outputPorts;

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

    // node

    internal static Wire AddWire(WireID type)
    {
        var node = new Wire { Id = _nextComponentId++, Type = type };
        _components[node.Id] = node;
        _wires.Add(node);
        return node;
    }

    internal static Wire AddWire(WireID type, int id)
    {
        var node = new Wire { Id = id, Type = type };
        _components[id] = node;
        _wires.Add(node);
        UpdateMaxId(id);
        return node;
    }

    internal static Gate AddGate(GateID type, (int x, int y) orgin)
    {
        var node = new Gate { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _gates.Add(node);
        return node;
    }

    internal static Gate AddGate(GateID type, int id)
    {
        var node = new Gate { Id = id, Type = type };
        _components[id] = node;
        _gates.Add(node);
        UpdateMaxId(id);
        return node;
    }

    internal static Lamp AddLamp(LampID type, (int x, int y) orgin)
    {
        var node = new Lamp { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _lamps.Add(node);
        return node;
    }

    internal static Lamp AddLamp(LampID type, int id)
    {
        var node = new Lamp { Id = id, Type = type };
        _components[id] = node;
        _lamps.Add(node);
        UpdateMaxId(id);
        return node;
    }

    internal static Input AddInput(InputID type, (int x, int y) orgin)
    {
        var node = new Input { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _inputs.Add(node);
        return node;
    }

    internal static InputPort AddInputPort()
    {
        var node = new InputPort { Id = _nextComponentId++ };
        _components[node.Id] = node;
        _inputPorts.Add(node);
        return node;
    }

    internal static InputPort AddInputPort(int id, int portId)
    {
        var node = new InputPort { Id = id, PortId = portId };
        _components[id] = node;
        _inputPorts.Add(node);
        UpdateMaxId(id);
        return node;
    }

    internal static Output AddOutput(OutputID type, (int x, int y) orgin)
    {
        var node = new Output { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _outputs.Add(node);
        return node;
    }

    internal static OutputPort AddOutputPort()
    {
        var node = new OutputPort { Id = _nextComponentId++ };
        _components[node.Id] = node;
        _outputPorts.Add(node);
        return node;
    }

    internal static OutputPort AddOutputPort(int id, int portId)
    {
        var node = new OutputPort { Id = id, PortId = portId };
        _components[id] = node;
        _outputPorts.Add(node);
        UpdateMaxId(id);
        return node;
    }

    private static void UpdateMaxId(int id)
    {
        if (id >= _nextComponentId) _nextComponentId = id + 1;
    }

    internal static IConnectable CopyNode(IConnectable node)
    {
        IConnectable copy = node switch
        {
            Wire w => AddWire(w.Type),
            Gate g => AddGate(g.Type, g.Origin),
            Lamp l => AddLamp(l.Type, l.Origin),
            Input i => AddInput(i.Type, i.Origin),
            Output o => AddOutput(o.Type, o.Origin),
            InputPort => AddInputPort(),
            OutputPort => AddOutputPort(),
            _ => node
        };

        foreach (var source in node.Fanin)
            AddEdge(source, copy);

        foreach (var target in node.Fanout)
            AddEdge(copy, target);

        return copy;
    }

    internal static void RemoveNode(IConnectable node)
    {
        foreach (var source in node.Fanin)
            source.Fanout.Remove(node);

        foreach (var target in node.Fanout)
            target.Fanin.Remove(node);

        node.Fanin.Clear();
        node.Fanout.Clear();

        _components.Remove(node.Id);

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

        GatePos.Clear();
        LampPos.Clear();
        InputPos.Clear();
        OutputPos.Clear();

        _components.Clear();
        _nextComponentId = 0;

        Array.Clear(_hash);

        WiringExtra.Clean();
        WiringTemp.Clean();
    }
}
