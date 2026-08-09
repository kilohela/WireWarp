using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared;

public abstract class Access
{
    public static Access Instance { get; set; } = null!;

    // Preprocess

    public abstract int MaxTilesX { get; }
    public abstract int MaxTilesY { get; }
    public abstract Tile Tile(int x, int y);

    // Runtime

    public abstract void ExecuteInput(InputID type, IOGraph iOGraph, int i, int j);
    public abstract void ExecuteOutput(OutputID type, IOGraph iOGraph, int i, int j);
}
