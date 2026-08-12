using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public static class IOGraph
{
    private static readonly byte[] _hash = new byte[32];

    private static readonly Dictionary<(int x, int y), (int portId, InputID type)> _inputs = [];
    private static readonly Dictionary<int, ((int x, int y), OutputID type)> _outputs = [];

    public static ReadOnlyMemory<byte> Hash => _hash;

    public static IReadOnlyDictionary<(int x, int y), (int portId, InputID type)> Inputs => _inputs;
    public static IReadOnlyDictionary<int, ((int x, int y) pos, OutputID type)> Outputs => _outputs;

    internal static void SetHash(byte[] hash) => hash.CopyTo(_hash, 0);

    internal static void SetInput((int x, int y) pos, int portId, InputID type) =>
        _inputs[pos] = (portId, type);

    internal static void SetOutput(int portId, (int x, int y) pos, OutputID type) =>
        _outputs[portId] = (pos, type);

    public static void Build()
    {
        Clean();

        foreach (var (pos, input) in WiringGraph.InputPos)
        {
            var ip = input.Fanout.OfType<InputPort>().FirstOrDefault();
            if (ip != null) _inputs[pos] = (ip.PortId, input.Type);
        }

        foreach (var op in WiringGraph.OutputPorts)
        {
            var output = op.Fanout.OfType<Output>().First();
            var wire = op.Fanin.OfType<Wire>().First();
            var pos = wire.Drains.First(d => WiringGraph.OutputPos[d] == output);
            _outputs[op.PortId] = (pos, output.Type);
        }

        SetHash(WiringGraph.Hash.Span.ToArray());

        IOExtra.Build();
    }

    public static void Clean()
    {
        _inputs.Clear();
        _outputs.Clear();
        
        IOExtra.Clean();
    }
}
