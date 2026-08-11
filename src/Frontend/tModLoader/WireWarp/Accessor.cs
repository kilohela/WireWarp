using WireWarp.Frontend.Shared;
using WireWarp.Frontend.Shared.ID;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.tModLoader.IO;

namespace WireWarp.Frontend.tModLoader;

internal sealed class Accessor : Access
{
    public override int MaxTilesX => Terraria.Main.maxTilesX;
    public override int MaxTilesY => Terraria.Main.maxTilesY;
    public override string WorldPathName => Terraria.Main.worldPathName;

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

    public override void Execute(InputID type, int portId, int i, int j) => RuntimeInput.Execute(type, portId, i, j);
    public override void Execute(OutputID type, int portId, int i, int j) => RuntimeOutput.Execute(type, portId, i, j);

    public override void Tick() => RuntimeGeneral.Tick();
    public override void Reset() => RuntimeGeneral.Reset();
}
