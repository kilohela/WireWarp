using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Assign
{
    public static void Execute(WiringGraph graph)
    {
        AssignPortIds(graph);
    }

    private static void AssignPortIds(WiringGraph graph)
    {
        for (var i = 0; i < graph.InputPorts.Count; i++)
            graph.InputPorts[i].PortId = i;

        for (var i = 0; i < graph.OutputPorts.Count; i++)
            graph.OutputPorts[i].PortId = i;
    }
}
