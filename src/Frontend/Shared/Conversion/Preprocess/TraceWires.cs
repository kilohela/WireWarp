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
            if (!Detector.HasWire(Main.tile(start.x, start.y), color)) continue;
            if (wireByTile.ContainsKey((start, color))) continue;

            var wire = graph.AddWire(color);
            var found = TraceWire(wire, start, start, graph, wireByTile);
            ConnectComponents(wire, found, graph);
        }
    }

    private static void ConnectComponents(
        Wire wire,
        List<IConnectable> found,
        WiringGraph graph)
    {
        foreach (var component in found.Distinct())
        {
            switch (component)
            {
                case Lamp lamp:
                    WiringGraph.AddEdge(wire, lamp);
                    break;

                case Gate gate:
                    for (var y = gate.Y - 1; ; y--)
                    {
                        if (graph.LampPos.TryGetValue((gate.X, y), out var gateLamp))
                            WiringGraph.AddEdge(gateLamp, gate);
                        else
                            break;
                    }
                    WiringGraph.AddEdge(gate, wire);
                    break;

                case Input input:
                    var ip = input.Fanout.OfType<InputPort>().FirstOrDefault() ?? 
                        graph.AddInputPort(input.X, input.Y);
                    WiringGraph.AddEdge(input, ip);
                    WiringGraph.AddEdge(ip, wire);
                    break;

                case Output output:
                    foreach (var c in found)
                    {
                        switch (c)
                        {
                            case Gate g:
                                var op1 = graph.AddOutputPort(g.X, g.Y);
                                WiringGraph.AddEdge(wire, op1);
                                WiringGraph.AddEdge(op1, output);
                                break;
                            case Input i:
                                var op2 = graph.AddOutputPort(i.X, i.Y);
                                WiringGraph.AddEdge(wire, op2);
                                WiringGraph.AddEdge(op2, output);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public static List<IConnectable> TraceWire(
        Wire wire,
        (int x, int y) start,
        (int x, int y) prevStart,
        WiringGraph graph,
        Dictionary<((int, int), WireID), Wire> wireByTile)
    {
        var found = new List<IConnectable>();

        var queue = new Queue<((int x, int y) cur, (int x, int y) prev)>();
        queue.Enqueue((start, prevStart));

        while (queue.Count > 0)
        {
            var (cur, prev) = queue.Dequeue();

            if (cur.x < 0 || cur.x >= Main.maxTilesX ||
                cur.y < 0 || cur.y >= Main.maxTilesY)
                continue;

            var tile = Main.tile(cur.x, cur.y);
            if (!Detector.HasWire(tile, wire.Type)) continue;

            var jb = Detector.DetectJunctionBox(tile);
            if (jb == JunctionBoxID.None && wireByTile.ContainsKey((cur, wire.Type)))
                continue;

            wireByTile[(cur, wire.Type)] = wire;

            if (graph.LampPos.TryGetValue(cur, out var lamp))
                found.Add(lamp);
            if (graph.GatePos.TryGetValue(cur, out var gate))
                found.Add(gate);
            if (graph.InputPos.TryGetValue(cur, out var input))
                found.Add(input);
            if (graph.OutputPos.TryGetValue(cur, out var output))
                found.Add(output);

            if (jb != JunctionBoxID.None)
            {
                var next = RouteJunction(cur, prev, jb);
                queue.Enqueue((next, cur));
            }
            else
            {
                var prevJb = Detector.DetectJunctionBox(Main.tile(prev.x, prev.y)) != JunctionBoxID.None;

                foreach (var (dx, dy) in new[] { (1, 0), (0, 1), (-1, 0), (0, -1) })
                {
                    var next = (x: cur.x + dx, y: cur.y + dy);
                    if (prevJb && prev == next) continue;
                    queue.Enqueue((next, cur));
                }
            }
        }

        return found;
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
