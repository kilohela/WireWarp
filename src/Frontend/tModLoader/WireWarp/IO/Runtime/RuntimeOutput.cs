using System.Reflection;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.tModLoader.IO;

internal static partial class RuntimeOutput
{
    private static readonly Action<IOGraph, int, int>?[] _outputs =
        new Action<IOGraph, int, int>?[Enum.GetValues<OutputID>().Length];

    static RuntimeOutput()
    {
        foreach (var method in typeof(RuntimeOutput).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<OutputID>(method.Name, out var id))
                _outputs[(int)id] = method.CreateDelegate<Action<IOGraph, int, int>>();
        }
    }

    public static void Execute(OutputID type, IOGraph iOGraph, int i, int j)
        => _outputs[(int)type]?.Invoke(iOGraph, i, j);
}
