using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Data;

public static class IOExtra
{
    private static readonly Dictionary<int, ((int x, int y) source, (int x, int y) target)> _teleporter = [];
    private static readonly Dictionary<int, (List<(int x, int y)> inlets, List<(int x, int y)> outlets)> _pumps = [];
    private static readonly Dictionary<int, WireID> _wireBulb = [];

    public static IReadOnlyDictionary<int, ((int x, int y) source, (int x, int y) target)> Teleporter => _teleporter;
    public static IReadOnlyDictionary<int, (List<(int x, int y)> inlets, List<(int x, int y)> outlets)> Pumps => _pumps;
    public static IReadOnlyDictionary<int, WireID> WireBulb => _wireBulb;

    public static void Build()
    {
        Clean();

        foreach (var (op, v) in WiringExtra.Teleporter) _teleporter[op.PortId] = v;
        foreach (var (op, v) in WiringExtra.Pumps) _pumps[op.PortId] = v;
        foreach (var (op, v) in WiringExtra.WireBulb) _wireBulb[op.PortId] = v;
    }

    public static void Clean()
    {
        _teleporter.Clear();
        _pumps.Clear();
        _wireBulb.Clear();
    }
}
