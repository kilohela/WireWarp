using System.Reflection;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.tModLoader.IO;

internal static partial class RuntimeInput
{
    private static readonly Action<int, int>?[] _inputs =
        new Action<int, int>?[Enum.GetValues<InputID>().Length];

    static RuntimeInput()
    {
        foreach (var method in typeof(RuntimeInput).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<InputID>(method.Name, out var id))
                _inputs[(int)id] = method.CreateDelegate<Action<int, int>>();
        }
    }

    public static void Execute(InputID type, int i, int j)
        => _inputs[(int)type]?.Invoke(i, j);
}
