using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public class WiringGraph
{
    public WiringExtra WiringExtra { get; } = new();
    public WiringTemp WiringTemp { get; } = new();

    private readonly Dictionary<int, IConnectable> _components = [];
    private int _nextComponentId;

    private readonly List<Wire> _wires = [];
    private readonly List<Gate> _gates = [];
    private readonly List<Lamp> _lamps = [];
    private readonly List<Input> _inputs = [];
    private readonly List<InputPort> _inputPorts = [];
    private readonly List<Output> _outputs = [];
    private readonly List<OutputPort> _outputPorts = [];

    public IReadOnlyDictionary<int, IConnectable> Components => _components;

    public IReadOnlyList<Wire> Wires => _wires;
    public IReadOnlyList<Gate> Gates => _gates;
    public IReadOnlyList<Lamp> Lamps => _lamps;
    public IReadOnlyList<Input> Inputs => _inputs;
    public IReadOnlyList<InputPort> InputPorts => _inputPorts;
    public IReadOnlyList<Output> Outputs => _outputs;
    public IReadOnlyList<OutputPort> OutputPorts => _outputPorts;
    
    internal Dictionary<(int x, int y), Gate> GatePos { get; } = [];
    internal Dictionary<(int x, int y), Lamp> LampPos { get; } = [];
    internal Dictionary<(int x, int y), Input> InputPos { get; } = [];
    internal Dictionary<(int x, int y), Output> OutputPos { get; } = [];

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

    internal Wire AddWire(WireID type)
    {
        var node = new Wire { Id = _nextComponentId++, Type = type };
        _components[node.Id] = node;
        _wires.Add(node);
        return node;
    }

    internal Gate AddGate(GateID type, (int x, int y) orgin)
    {
        var node = new Gate { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _gates.Add(node);
        return node;
    }

    internal Lamp AddLamp(LampID type, (int x, int y) orgin)
    {
        var node = new Lamp { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _lamps.Add(node);
        return node;
    }

    internal Input AddInput(InputID type, (int x, int y) orgin)
    {
        var node = new Input { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _inputs.Add(node);
        return node;
    }

    internal InputPort AddInputPort()
    {
        var node = new InputPort { Id = _nextComponentId++ };
        _components[node.Id] = node;
        _inputPorts.Add(node);
        return node;
    }

    internal Output AddOutput(OutputID type, (int x, int y) orgin)
    {
        var node = new Output { Id = _nextComponentId++, Type = type, Origin = orgin };
        _components[node.Id] = node;
        _outputs.Add(node);
        return node;
    }

    internal OutputPort AddOutputPort()
    {
        var node = new OutputPort { Id = _nextComponentId++ };
        _components[node.Id] = node;
        _outputPorts.Add(node);
        return node;
    }

    internal IConnectable CopyNode(IConnectable node)
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

    internal void RemoveNode(IConnectable node)
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
}
