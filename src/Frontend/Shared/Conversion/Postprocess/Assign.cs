using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Assign
{
    public static void Execute()
    {
        AssignPortIds();
    }

    private static void AssignPortIds()
    {
        for (var i = 0; i < WiringGraph.InputPorts.Count; i++)
            WiringGraph.InputPorts[i].PortId = i;

        for (var i = 0; i < WiringGraph.OutputPorts.Count; i++)
            WiringGraph.OutputPorts[i].PortId = i;
    }
}
