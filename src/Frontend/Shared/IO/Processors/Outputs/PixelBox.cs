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
            sources.Add(source);

        var horizontal = new HashSet<IConnectable>();
        var vertical = new HashSet<IConnectable>();

        foreach (var color in new[] { WireID.Red, WireID.Blue, WireID.Green, WireID.Yellow })
        {
            if (!Conversion.Detector.HasWire(Main.tile[output.X, output.Y], color)) continue;

            TraceDir((output.X - 1, output.Y), (output.X, output.Y), sources, horizontal, color, graph);
            TraceDir((output.X + 1, output.Y), (output.X, output.Y), sources, horizontal, color, graph);
            TraceDir((output.X, output.Y - 1), (output.X, output.Y), sources, vertical, color, graph);
            TraceDir((output.X, output.Y + 1), (output.X, output.Y), sources, vertical, color, graph);
        }

        horizontal.IntersectWith(vertical);

        var visitedSource = new HashSet<IConnectable>();
        foreach (var op in output.Fanin.OfType<OutputPort>().ToList())
        {
            var source = op.Fanin.OfType<Wire>().First().Fanin
                .First(s => (s is Gate g && g.X == op.X && g.Y == op.Y)
                         || (s is InputPort ip && ip.X == op.X && ip.Y == op.Y));
            if (!horizontal.Contains(source) || visitedSource.Contains(source))
                graph.RemoveNode(op);
            else
                visitedSource.Add(source);
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

        var found = Conversion.TraceWires.TraceWire(
            wire, start, prev, graph, visited);

        foreach (var component in found)
        {
            if (component is Gate gate && sources.Contains(gate))
                result.Add(gate);
            else if (component is Input input)
            {
                var ip = input.Fanout.OfType<InputPort>().First();
                if (sources.Contains(ip))
                    result.Add(ip);
            }
        }
    }
}
