using System.Reflection;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

internal static partial class ProcessOutput
{
    private static readonly Action<Output>?[] _processors =
        new Action<Output>?[Enum.GetValues<OutputID>().Length];

    static ProcessOutput()
    {
        foreach (var method in typeof(ProcessOutput).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<OutputID>(method.Name, out var id))
                _processors[(int)id] = method.CreateDelegate<Action<Output>>();
        }
    }

    public static void Execute()
    {
        foreach (var output in WiringGraph.Outputs)
            _processors[(int)output.Type]?.Invoke(output);
    }
}
