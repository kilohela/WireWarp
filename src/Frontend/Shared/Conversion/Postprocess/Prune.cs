using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Prune
{
    public static void Execute()
    {
        bool changed;
        do
        {
            changed = false;
            changed |= PruneWhere(WiringGraph.Wires);
            changed |= PruneWhere(WiringGraph.Gates);
            changed |= PruneWhere(WiringGraph.Lamps);
            changed |= PruneWhere(WiringGraph.Inputs);
            changed |= PruneWhere(WiringGraph.Outputs);
            changed |= PruneWhere(WiringGraph.InputPorts);
            changed |= PruneWhere(WiringGraph.OutputPorts);
        }
        while (changed);
    }

    private static bool PruneWhere<T>(IReadOnlyList<T> nodes) where T : IConnectable
    {
        var removed = false;
        for (var i = nodes.Count - 1; i >= 0; i--)
        {
            if (IsDead(nodes[i]))
            {
                WiringGraph.RemoveNode(nodes[i]);
                removed = true;
            }
        }
        return removed;
    }

    private static bool IsDead(IConnectable node) => node switch
    {
        Wire or Gate or InputPort or OutputPort =>
            node.Fanin.Count == 0 || node.Fanout.Count == 0,
        Lamp or Input => node.Fanout.Count == 0,
        Output => node.Fanin.Count == 0,
        _ => false,
    };
}
