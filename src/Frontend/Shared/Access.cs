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

    public abstract void ExecuteInput(InputID type, int i, int j, int portId);
    public abstract void ExecuteOutput(OutputID type, int i, int j, int portId);
}
