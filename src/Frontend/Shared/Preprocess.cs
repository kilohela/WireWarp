using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared;

public static class Preprocess
{
    public static void Execute()
    {
        var hash = WiringHash.GetHash();

        if (!WiringFile.MatchHash(hash) || !IOFile.MatchHash(hash))
        {
            WiringGraph.SetHash(hash);
            WiringGraph.Build();
            IOGraph.Build();

            WiringFile.Save();
            IOFile.Save();

            WiringGraph.Clean();
        }
        else
        {
            IOFile.Load();
        }

        WWorldFile.Save();
    }
}
