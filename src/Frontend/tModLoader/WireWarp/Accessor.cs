using WireWarp.Frontend.Shared;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.tModLoader;

internal sealed class Accessor : Access
{
    public override int MaxTilesX => Terraria.Main.maxTilesX;
    public override int MaxTilesY => Terraria.Main.maxTilesY;

    public override Tile Tile(int x, int y)
    {
        var real = Terraria.Main.tile[x, y];
        return new Tile
        {
            type = real.TileType,
            frameX = real.TileFrameX,
            frameY = real.TileFrameY,
            HasTile = real.HasTile,
            HasActuator = real.HasActuator,
            IsActuated = real.IsActuated,
            RedWire = real.RedWire,
            BlueWire = real.BlueWire,
            GreenWire = real.GreenWire,
            YellowWire = real.YellowWire,
        };
    }
}
