using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.Conversion;

public static class Converter
{
    public static void Execute()
    {
        WiringGraph.Clean();

        // preprocess
        ScanComponents.Execute();
        TraceWires.Execute();

        // postprocess
        Prune.Execute();
        Normalize.Execute();
        Prune.Execute();
        Applier.Execute();
        Prune.Execute();
        Assign.Execute();
        Validate.Execute();

        // io graph
        IOGraph.Build();
        WiringGraph.Clean();
    }
}
