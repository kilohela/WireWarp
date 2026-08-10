using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared;

public static class Converter
{
    public static void Execute()
    {
        WiringGraph.Build();
        IOGraph.Build();

        WiringGraph.Clean();
        IOFrame.Clean();
    }
}
