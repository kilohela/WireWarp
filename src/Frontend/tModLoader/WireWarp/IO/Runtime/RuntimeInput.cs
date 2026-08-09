using System.Reflection;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.tModLoader.IO;

internal static partial class RuntimeInput
{
    private static readonly Action<IOGraph, int, int>?[] _inputs =
        new Action<IOGraph, int, int>?[Enum.GetValues<InputID>().Length];

    static RuntimeInput()
    {
        foreach (var method in typeof(RuntimeInput).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<InputID>(method.Name, out var id))
                _inputs[(int)id] = method.CreateDelegate<Action<IOGraph, int, int>>();
        }
    }

    public static void Execute(InputID type, IOGraph iOGraph, int i, int j)
        => _inputs[(int)type]?.Invoke(iOGraph, i, j);
}
