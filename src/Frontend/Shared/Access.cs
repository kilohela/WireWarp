using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared;

public abstract class Access
{
    public static Access Instance { get; set; } = null!;

    public abstract int MaxTilesX { get; }
    public abstract int MaxTilesY { get; }
    public abstract Tile Tile(int x, int y);
}
