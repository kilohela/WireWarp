using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Scan
{
    public static void Execute()
    {
        ScanComponents();
    }

    private static void ScanComponents()
    {
        var inputByOrigin = new Dictionary<(int x, int y, InputID type), Input>();
        var outputByOrigin = new Dictionary<(int x, int y, OutputID type), Output>();

        var w = Access.Instance.MaxTilesX;
        var h = Access.Instance.MaxTilesY;

        for (var x = 0; x < w; x++)
        {
            if (x % Math.Max(1, w / 100) == 0)
                Access.Instance.Status($"Scanning components {x * 1f / w:P1}");
            for (var y = 0; y < h; y++)
            {
                var tile = Access.Instance.GetTile(x, y);
                if (!tile.HasTile) continue;

                var gateType = Detector.DetectGate(tile);
                if (gateType != GateID.None)
                {
                    WiringGraph.GatePos[(x, y)] = WiringGraph.AddGate(gateType, (x, y));
                    continue;
                }

                var lampType = Detector.DetectLamp(tile);
                if (lampType != LampID.None)
                {
                    WiringGraph.LampPos[(x, y)] = WiringGraph.AddLamp(lampType, (x, y));
                    continue;
                }

                var inputType = Detector.DetectInput(tile);
                if (inputType != InputID.None)
                {
                    var origin = Detector.GetInputOrigin(inputType, x, y, tile.frameX, tile.frameY);
                    var size = Detector.GetInputSize(inputType);
                    var inRange = InRange(x, y, origin, size);
                    var key = (origin.x, origin.y, inputType);

                    var input = inRange && inputByOrigin.TryGetValue(key, out var merged)
                        ? merged
                        : WiringGraph.AddInput(inputType, origin);

                    if (inRange) inputByOrigin[key] = input;
                    WiringGraph.InputPos[(x, y)] = input;
                }

                var outputType = Detector.DetectOutput(tile);
                if (outputType != OutputID.None)
                {
                    var origin = Detector.GetOutputOrigin(outputType, x, y, tile.frameX, tile.frameY);
                    var size = Detector.GetOutputSize(outputType);
                    var inRange = InRange(x, y, origin, size);
                    var key = (origin.x, origin.y, outputType);

                    var output = inRange && outputByOrigin.TryGetValue(key, out var merged)
                        ? merged
                        : WiringGraph.AddOutput(outputType, origin);

                    if (inRange) outputByOrigin[key] = output;
                    WiringGraph.OutputPos[(x, y)] = output;
                }
            }
        }
    }

    private static bool InRange(int x, int y, (int x, int y) origin, (int x, int y) size)
        => x >= origin.x && x < origin.x + size.x
        && y >= origin.y && y < origin.y + size.y;
}
