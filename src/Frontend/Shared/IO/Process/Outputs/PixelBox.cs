using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.IO;

partial class ProcessOutput
{
    private static void PixelBox(WiringGraph graph, Output output)
    {
        var sources = new HashSet<IConnectable>();
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            foreach (var source in op.Fanin.OfType<Wire>().First().Fanin)
                sources.Add(source is InputPort ip 
                    ? ip.Fanin.OfType<Input>().First() 
                    : source);
            graph.RemoveNode(op);
        }

        var horizontal = new Dictionary<IConnectable, (int x, int y)>();
        var vertical = new Dictionary<IConnectable, (int x, int y)>();

        var o = output.Origin;
        foreach (var color in new[] { WireID.Red, WireID.Blue, WireID.Green, WireID.Yellow })
        {
            if (!Conversion.Detector.HasWire(Main.tile[o.X, o.Y], color)) continue;

            TraceDir((o.X - 1, o.Y), o, sources, horizontal, color, graph);
            TraceDir((o.X + 1, o.Y), o, sources, horizontal, color, graph);
            TraceDir((o.X, o.Y - 1), o, sources, vertical, color, graph);
            TraceDir((o.X, o.Y + 1), o, sources, vertical, color, graph);
        }

        var intersection = horizontal
            .Where(kv => vertical.ContainsKey(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var source in intersection)
        {
            var wire = graph.AddWire(WireID.Red);
            var port = graph.AddOutputPort(source.Value, o);

            WiringGraph.AddEdge(source.Key is Input input 
                ? input.Fanout.OfType<InputPort>().First() 
                : source.Key, wire);
            WiringGraph.AddEdge(wire, port);
            WiringGraph.AddEdge(port, output);
        }
    }

    private static void TraceDir(
        (int x, int y) start, (int x, int y) prev,
        HashSet<IConnectable> sources,
        Dictionary<IConnectable, (int x, int y)> result,
        WireID color,
        WiringGraph graph)
    {
        var wire = new Wire { Type = color };
        var visited = new Dictionary<((int, int), WireID), Wire>();

        var founds = Conversion.TraceWires
            .TraceWire(wire, start, prev, graph, visited)
            .Where(f => sources
            .Contains(f.component));

        foreach (var (active, component) in founds)
            result.TryAdd(component, active);
    }
}
