using System.Reflection;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

internal static partial class Processor
{
    private static readonly Action<WiringGraph, Output>?[] _processors =
        new Action<WiringGraph, Output>?[Enum.GetValues<OutputID>().Length];

    static Processor()
    {
        foreach (var method in typeof(Processor).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<OutputID>(method.Name, out var id))
                _processors[(int)id] = method.CreateDelegate<Action<WiringGraph, Output>>();
        }
    }

    public static void Execute(WiringGraph graph)
    {
        foreach (var output in graph.Outputs)
            _processors[(int)output.Type]?.Invoke(graph, output);
    }
}
