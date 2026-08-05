using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Validate
{
    [Conditional("DEBUG")]
    public static void Execute(WiringGraph graph)
    {
        ValidateConstraints(graph);
        ValidateSymmetry(graph);
    }

    private static void ValidateConstraints(WiringGraph graph)
    {
        foreach (var node in graph.Components.Values)
        {
            switch (node)
            {
                case Input:
                    Debug.Assert(node.Fanin.Count == 0,
                        $"{At(node)} Fanin expect 0, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is InputPort),
                        $"{At(node)} Fanout expect InputPort");
                    break;

                case InputPort:
                    Debug.Assert(node.Fanin.Count == 1,
                        $"{At(node)} Fanin expect 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Input),
                        $"{At(node)} Fanin expect Input");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Wire),
                        $"{At(node)} Fanout expect Wire");
                    break;

                case Output:
                    Debug.Assert(node.Fanout.Count == 0,
                        $"{At(node)} Fanout expect 0, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is OutputPort),
                        $"{At(node)} Fanin expect OutputPort");
                    break;

                case OutputPort:
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Wire),
                        $"{At(node)} Fanin expect Wire");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Output),
                        $"{At(node)} Fanout expect Output");
                    break;

                case Lamp:
                    Debug.Assert(node.Fanin.All(x => x is Wire),
                        $"{At(node)} Fanin expect Wire");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Gate),
                        $"{At(node)} Fanout expect Gate");
                    break;

                case Gate:
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Lamp),
                        $"{At(node)} Fanin expect Lamp");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Wire),
                        $"{At(node)} Fanout expect Wire");
                    break;

                case Wire:
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Gate || x is InputPort),
                        $"{At(node)} Fanin expect Gate or InputPort");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Lamp || x is OutputPort),
                        $"{At(node)} Fanout expect Lamp or OutputPort");
                    break;
            }
        }
    }

    private static void ValidateSymmetry(WiringGraph graph)
    {
        foreach (var node in graph.Components.Values)
        {
            foreach (var target in node.Fanout)
                Debug.Assert(target.Fanin.Contains(node),
                    $"{At(node)} edge asymmetry: {At(target)}");

            foreach (var source in node.Fanin)
                Debug.Assert(source.Fanout.Contains(node),
                    $"{At(source)} edge asymmetry: {At(node)}");
        }
    }

    private static string At(IConnectable node) => node switch
    {
        Input i => $"Input#{i.Id}@({i.X},{i.Y})",
        InputPort ip => $"InputPort#{ip.Id}@({ip.X},{ip.Y})",
        Output o => $"Output#{o.Id}@({o.X},{o.Y})",
        OutputPort op => $"OutputPort#{op.Id}@({op.X},{op.Y})",
        Lamp l => $"Lamp#{l.Id}@({l.X},{l.Y})",
        Gate g => $"Gate#{g.Id}@({g.X},{g.Y})",
        Wire w => $"Wire#{w.Id}",
        _ => $"#{node.Id}"
    };
}
