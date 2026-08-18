using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Reporter
{
    public static void Execute()
    {
        Report.Components.Clear();
        Report.Histograms.Clear();
        Report.Edges = 0;

        Report.Components["Input"] = CountByType(WiringGraph.Inputs, n => ((InputID)n.Type).ToString());
        Report.Components["InputPort"] = CountByType(WiringGraph.InputPorts, n => ((InputID)n.Type).ToString());
        Report.Components["Output"] = CountByType(WiringGraph.Outputs, n => ((OutputID)n.Type).ToString());
        Report.Components["OutputPort"] = CountByType(WiringGraph.OutputPorts, n => ((OutputID)n.Type).ToString());
        Report.Components["Lamp"] = CountByType(WiringGraph.Lamps, n => ((LampID)n.Type).ToString());
        Report.Components["Gate"] = CountByType(WiringGraph.Gates, n => ((GateID)n.Type).ToString());
        Report.Components["Wire"] = CountByType(WiringGraph.Wires, n => ((WireID)n.Type).ToString());

        foreach (var node in WiringGraph.Components.Values)
            Report.Edges += node.Fanout.Count;

        foreach (var wire in WiringGraph.Wires)
        {
            AddHistogram("Wire fanin", wire.Fanin.Count);
            AddHistogram("Wire fanout", wire.Fanout.Count);
        }

        foreach (var lamp in WiringGraph.Lamps)
        {
            var name = $"Lamp fanin {lamp.Type}";
            AddHistogram(name, lamp.Fanin.OfType<Wire>().Count());
        }

        foreach (var gate in WiringGraph.Gates)
        {
            var prefix = gate.Type == GateID.Fault ? "Fault gate" : "Normal gate";
            var lamps = gate.Fanin.OfType<Lamp>().ToList();
            var faultLamps = lamps.Count(l => l.Type == LampID.Fault);

            AddHistogram($"{prefix} lamps", lamps.Count);
            AddHistogram($"{prefix} fault lamps", faultLamps);
            AddHistogram($"{prefix} wire fanout", gate.Fanout.OfType<Wire>().Count());

            foreach (var lamp in lamps)
            {
                var lampType = lamp.Type.ToString();
                AddHistogram($"{prefix} {lampType} lamp wire fanin",
                    lamp.Fanin.OfType<Wire>().Count());
            }
        }
    }

    private static Dictionary<string, int> CountByType(IEnumerable<IConnectable> nodes,
        Func<IConnectable, string> name)
    {
        var result = new Dictionary<string, int>();
        foreach (var node in nodes)
        {
            var key = name(node);
            result[key] = result.GetValueOrDefault(key) + 1;
        }
        return result;
    }

    private static void AddHistogram(string name, int count)
    {
        if (!Report.Histograms.TryGetValue(name, out var hist))
            Report.Histograms[name] = hist = [];
        hist[count] = hist.GetValueOrDefault(count) + 1;
    }
}
