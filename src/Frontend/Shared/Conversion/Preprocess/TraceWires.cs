using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class TraceWires
{
    public static void Execute(WiringGraph graph)
    {
        var wireByTile = new Dictionary<((int x, int y) pos, WireID color), Wire>();

        foreach (var pos in graph.InputPos.Keys)
            TraceSource(pos, graph, wireByTile);

        foreach (var pos in graph.GatePos.Keys)
            TraceSource(pos, graph, wireByTile);
    }

    private static void TraceSource(
        (int x, int y) start,
        WiringGraph graph,
        Dictionary<((int, int), WireID), Wire> wireByTile)
    {
        foreach (var color in new[] { WireID.Red, WireID.Blue, WireID.Green, WireID.Yellow })
        {
            if (!Detector.HasWire(Main.tile[start.x, start.y], color)) continue;
            if (wireByTile.ContainsKey((start, color))) continue;

            var wire = graph.AddWire(color);
            var founds = TraceWire(wire, start, start, graph, wireByTile);
            ConnectComponents(wire, founds, graph);
        }
    }

    private static void ConnectComponents(
        Wire wire,
        List<((int x, int y) active, IConnectable component)> founds,
        WiringGraph graph)
    {
        var componentByTile = new Dictionary<IConnectable, (int x, int y)>();
        foreach (var found in founds)
        {
            var component = found.component;
            if (componentByTile.ContainsKey(component)) continue;
            else componentByTile[component] = found.active;

            switch (component)
            {
                case Lamp lamp:
                    WiringGraph.AddEdge(wire, lamp);
                    break;

                case Gate gate:
                    WiringGraph.AddEdge(gate, wire);
                    for (var y = gate.Origin.Y - 1; ; y--)
                    {
                        if (graph.LampPos.TryGetValue((gate.Origin.X, y), out var gateLamp))
                            WiringGraph.AddEdge(gateLamp, gate);
                        else
                            break;
                    }
                    break;

                case Input input:
                    var ip = input.Fanout.OfType<InputPort>().FirstOrDefault() ?? 
                        graph.AddInputPort();
                    WiringGraph.AddEdge(input, ip);
                    WiringGraph.AddEdge(ip, wire);
                    break;
            }
        }

        foreach (var (output, drain) in componentByTile.Where(f => f.Key is Output))
        foreach (var (component, source) in componentByTile)
        {
            switch (component)
            {
                case Gate:
                    var op1 = graph.AddOutputPort(source, drain);
                    WiringGraph.AddEdge(wire, op1);
                    WiringGraph.AddEdge(op1, (Output)output);
                    break;
                case Input:
                    var op2 = graph.AddOutputPort(source, drain);
                    WiringGraph.AddEdge(wire, op2);
                    WiringGraph.AddEdge(op2, (Output)output);
                    break;
            }
        }
    }

    public static List<((int x, int y) active, IConnectable component)> TraceWire(
        Wire wire,
        (int x, int y) start,
        (int x, int y) prevStart,
        WiringGraph graph,
        Dictionary<((int, int), WireID), Wire> wireByTile)
    {
        var founds = new List<((int x, int y) active, IConnectable component)>();

        var queue = new Queue<((int x, int y) cur, (int x, int y) prev)>();
        queue.Enqueue((start, prevStart));

        while (queue.Count > 0)
        {
            var (cur, prev) = queue.Dequeue();

            if (cur.x < 0 || cur.x >= Main.maxTilesX ||
                cur.y < 0 || cur.y >= Main.maxTilesY)
                continue;

            var tile = Main.tile[cur.x, cur.y];
            if (!Detector.HasWire(tile, wire.Type)) continue;

            var jb = Detector.DetectJunctionBox(tile);
            if (jb == JunctionBoxID.None && wireByTile.ContainsKey((cur, wire.Type)))
                continue;

            wireByTile[(cur, wire.Type)] = wire;

            if (graph.LampPos.TryGetValue(cur, out var lamp))
                founds.Add((cur, lamp));
            if (graph.GatePos.TryGetValue(cur, out var gate))
                founds.Add((cur, gate));
            if (graph.InputPos.TryGetValue(cur, out var input))
                founds.Add((cur, input));
            if (graph.OutputPos.TryGetValue(cur, out var output))
                founds.Add((cur, output));

            if (jb != JunctionBoxID.None)
            {
                var next = RouteJunction(cur, prev, jb);
                queue.Enqueue((next, cur));
            }
            else
            {
                var prevJb = Detector.DetectJunctionBox(Main.tile[prev.x, prev.y]) != JunctionBoxID.None;

                foreach (var (dx, dy) in new[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
                {
                    var next = (x: cur.x + dx, y: cur.y + dy);
                    if (prevJb && prev == next) continue;
                    queue.Enqueue((next, cur));
                }
            }
        }

        return founds;
    }

    private static (int x, int y) RouteJunction(
        (int x, int y) cur,
        (int x, int y) prev,
        JunctionBoxID type)
    {
        return type switch
        {
            JunctionBoxID.UpDown => (
                cur.x + (cur.x - prev.x),
                cur.y + (cur.y - prev.y)),

            JunctionBoxID.UpLeft => (
                cur.x - (cur.y - prev.y),
                cur.y - (cur.x - prev.x)),

            JunctionBoxID.UpRight => (
                cur.x + (cur.y - prev.y),
                cur.y + (cur.x - prev.x)),

            _ => cur
        };
    }
}
