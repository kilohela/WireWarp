using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Assign
{
    public static void Execute()
    {
        Access.Instance.Status("Assigning wiring...");
        AssignPortIds();
    }

    private static void AssignPortIds()
    {
        var i = 0;
        foreach (var port in WiringGraph.InputPorts)
            port.PortId = i++;

        var j = 0;
        foreach (var port in WiringGraph.OutputPorts)
            port.PortId = j++;
    }
}
