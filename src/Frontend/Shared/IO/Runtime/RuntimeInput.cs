using System.Reflection;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.IO;

internal static partial class RuntimeInput
{
    private static readonly Action<int, int, int>?[] _inputs =
        new Action<int, int, int>?[Enum.GetValues<InputID>().Length];

    static RuntimeInput()
    {
        foreach (var method in typeof(RuntimeInput).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (Enum.TryParse<InputID>(method.Name, out var id))
                _inputs[(int)id] = method.CreateDelegate<Action<int, int, int>>();
        }
    }

    public static void Execute(InputID type, int i, int j, int portId)
        => _inputs[(int)type]?.Invoke(i, j, portId);
}
