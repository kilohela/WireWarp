using WireWarp.Frontend.Shared;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.tModLoader.IO;

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

    // Runtime

    public override void ExecuteInput(InputID type, int i, int j, int portId)
        => RuntimeInput.Execute(type, i, j, portId);

    public override void ExecuteOutput(OutputID type, int i, int j, int portId)
        => RuntimeOutput.Execute(type, i, j, portId);
}
