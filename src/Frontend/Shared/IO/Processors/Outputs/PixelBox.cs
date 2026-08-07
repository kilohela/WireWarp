using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.IO;

partial class Processor
{
    private static void PixelBox(WiringGraph graph, Output output)
    {
        var sources = new HashSet<IConnectable>();
        foreach (var op in output.Fanin.OfType<OutputPort>())
        foreach (var source in op.Fanin.OfType<Wire>().First().Fanin)
            sources.Add(source is InputPort ip ? ip.Fanin.OfType<Input>().First() : source);

        var horizontal = new HashSet<IConnectable>();
        var vertical = new HashSet<IConnectable>();

        var o = output.Origin;
        foreach (var color in new[] { WireID.Red, WireID.Blue, WireID.Green, WireID.Yellow })
        {
            if (!Conversion.Detector.HasWire(Main.tile[o.X, o.Y], color)) continue;

            TraceDir((o.X - 1, o.Y), o, sources, horizontal, color, graph);
            TraceDir((o.X + 1, o.Y), o, sources, horizontal, color, graph);
            TraceDir((o.X, o.Y - 1), o, sources, vertical, color, graph);
            TraceDir((o.X, o.Y + 1), o, sources, vertical, color, graph);
        }

        horizontal.IntersectWith(vertical);

        var visitedSource = new HashSet<IConnectable>();
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            var source = graph.GatePos.TryGetValue(op.Source, out var gate)
                ? (IConnectable)gate
                : graph.InputPos[op.Source];
            if (!horizontal.Contains(source) || !visitedSource.Add(source))
                graph.RemoveNode(op);
        }
    }

    private static void TraceDir(
        (int x, int y) start, (int x, int y) prev,
        HashSet<IConnectable> sources,
        HashSet<IConnectable> result,
        WireID color,
        WiringGraph graph)
    {
        var wire = new Wire { Type = color };
        var visited = new Dictionary<((int, int), WireID), Wire>();

        var founds = Conversion.TraceWires.TraceWire(
            wire, start, prev, graph, visited);

        result.UnionWith(founds
            .Select(f => f.component)
            .Where(sources.Contains));
    }
}
