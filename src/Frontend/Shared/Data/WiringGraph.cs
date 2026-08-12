using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Conversion;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared.Data;

public static class WiringGraph
{
    private static readonly byte[] _hash = new byte[32];

    private static readonly Dictionary<int, IConnectable> _components = [];
    private static int _nextComponentId;

    private static readonly List<Wire> _wires = [];
    private static readonly List<Gate> _gates = [];
    private static readonly List<Lamp> _lamps = [];
    private static readonly List<Input> _inputs = [];
    private static readonly List<InputPort> _inputPorts = [];
    private static readonly List<Output> _outputs = [];
    private static readonly List<OutputPort> _outputPorts = [];

    public static ReadOnlyMemory<byte> Hash => _hash;

    public static IReadOnlyDictionary<int, IConnectable> Components => _components;

    public static IReadOnlyList<Wire> Wires => _wires;
    public static IReadOnlyList<Gate> Gates => _gates;
    public static IReadOnlyList<Lamp> Lamps => _lamps;
    public static IReadOnlyList<Input> Inputs => _inputs;
    public static IReadOnlyList<InputPort> InputPorts => _inputPorts;
    public static IReadOnlyList<Output> Outputs => _outputs;
    public static IReadOnlyList<OutputPort> OutputPorts => _outputPorts;

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

        // preprocess
        Scan.Execute();
        Trace.Execute();

        // postprocess
        Prune.Execute();
        Normalize.Execute();
        Prune.Execute();
        Applier.Execute();
        Prune.Execute();
        Assign.Execute();
        Validate.Execute();
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
