using System.Text;
using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.File;

public static class ReportFile
{
    public static string PathName =>
        Path.ChangeExtension(Access.Instance.WorldPathName, ".md");

    private static string TempPathName => PathName + ".tmp";

    public static void Write()
    {
        var path = PathName;
        var temp = TempPathName;

        try
        {
            System.IO.File.WriteAllText(temp, Build());
            System.IO.File.Move(temp, path, overwrite: true);
        }
        catch (Exception e)
        {
            try { if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp); } catch { }
            throw new Exception($"Failed to write report {path}: {e.Message}", e);
        }
    }

    private static string Build()
    {
        var sb = new StringBuilder();

        var hash = Report.Hash is { Length: > 0 } h ? Convert.ToHexString(h) : "-";
        var total = Report.Stages.Count > 0 ? Report.Stages.Sum(s => s.time) : 0;

        sb.AppendLine("# WireWarp Wiring Report");
        sb.AppendLine();
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| World | `{Report.WorldPath}` |");
        sb.AppendLine($"| Hash | `{hash}` |");
        sb.AppendLine($"| Result | {(Report.Success ? "OK" : "FAILED")} |");
        sb.AppendLine($"| Total ms | {total:F2} |");
        sb.AppendLine();

        sb.AppendLine("## Stages");
        sb.AppendLine();
        sb.AppendLine("| Stage | ms |");
        sb.AppendLine("|---|---|");
        if (Report.Stages.Count == 0)
            sb.AppendLine("| - | - |");
        else
            foreach (var (name, ms) in Report.Stages)
                sb.AppendLine($"| {name} | {ms:F2} |");
        sb.AppendLine();

        sb.AppendLine("## Components");
        sb.AppendLine();
        sb.AppendLine("| Component | Count | Type |");
        sb.AppendLine("|---|---|---|");
        if (Report.Components.Count == 0)
            sb.AppendLine("| - | - | - |");
        else
            foreach (var (component, types) in Report.Components)
            {
                var count = types.Values.Sum();
                var byType = types.Count == 0
                    ? "-"
                    : string.Join(", ", types.OrderBy(kv => kv.Key)
                        .Select(kv => $"{kv.Key}×{kv.Value}"));
                sb.AppendLine($"| {component} | {count} | {byType} |");
            }
        sb.AppendLine();

        sb.AppendLine("## Topology");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Edges | {Report.Edges} |");
        foreach (var (component, count) in Report.Pruned.OrderBy(kv => kv.Key))
            sb.AppendLine($"| Pruned {component} | {count} |");
        foreach (var (name, histogram) in Report.Histograms
                     .Where(kv => !kv.Key.StartsWith("Normal gate") &&
                                  !kv.Key.StartsWith("Fault gate"))
                     .OrderBy(kv => kv.Key))
            sb.AppendLine($"| {name} distribution | {FormatHistogram(histogram)} |");
        sb.AppendLine();

        var gateTypes = Report.Components.TryGetValue("Gate", out var gates)
            ? gates
            : [];
        var normalGates = gateTypes.Where(kv => kv.Key != "Fault").Sum(kv => kv.Value);
        var faultGates = gateTypes.GetValueOrDefault("Fault");

        sb.AppendLine("## Normal Gates");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Count | {normalGates} |");
        sb.AppendLine($"| Lamp fanin distribution | {Histogram("Normal gate lamps")} |");
        sb.AppendLine($"| Fault lamp count distribution | {Histogram("Normal gate fault lamps")} |");
        sb.AppendLine($"| Wire fanout distribution | {Histogram("Normal gate wire fanout")} |");
        sb.AppendLine($"| On lamp wire fanin distribution | {Histogram("Normal gate On lamp wire fanin")} |");
        sb.AppendLine($"| Off lamp wire fanin distribution | {Histogram("Normal gate Off lamp wire fanin")} |");
        sb.AppendLine($"| Fault lamp wire fanin distribution | {Histogram("Normal gate Fault lamp wire fanin")} |");
        sb.AppendLine();

        sb.AppendLine("## Fault Gates");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Count | {faultGates} |");
        sb.AppendLine($"| Lamp fanin distribution | {Histogram("Fault gate lamps")} |");
        sb.AppendLine($"| Fault lamp count distribution | {Histogram("Fault gate fault lamps")} |");
        sb.AppendLine($"| Wire fanout distribution | {Histogram("Fault gate wire fanout")} |");
        sb.AppendLine($"| On lamp wire fanin distribution | {Histogram("Fault gate On lamp wire fanin")} |");
        sb.AppendLine($"| Off lamp wire fanin distribution | {Histogram("Fault gate Off lamp wire fanin")} |");
        sb.AppendLine($"| Fault lamp wire fanin distribution | {Histogram("Fault gate Fault lamp wire fanin")} |");
        sb.AppendLine();

        if (Report.Errors.Count > 0)
        {
            sb.AppendLine("## Errors");
            sb.AppendLine();
            sb.AppendLine("| # | Message |");
            sb.AppendLine("|---|---|");
            for (var i = 0; i < Report.Errors.Count; i++)
                sb.AppendLine($"| {i + 1} | {Report.Errors[i]} |");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Histogram(string name) => 
        Report.Histograms.TryGetValue(name, out var histogram) ? FormatHistogram(histogram) : "-";

    private static string FormatHistogram(Dictionary<int, int> histogram) =>
        histogram.Count != 0 ? string.Join(", ", histogram
            .OrderByDescending(kv => kv.Key)
            .Select(kv => $"{kv.Key}->{kv.Value}")) : "-";
}
