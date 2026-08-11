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
    public abstract string WorldPathName { get; }

    // Runtime

    public abstract void Execute(InputID type, int portId, int i, int j);
    public abstract void Execute(OutputID type, int portId, int i, int j);

    public abstract void Tick();
    public abstract void Reset();
}
