using System.Buffers.Binary;
using System.Security.Cryptography;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.File;

public static class WiringHash
{
    public const int TileCellSize = 7;

    public static void GetTileHash(Span<byte> dst, Tile tile)
    {
        byte flags = 0;

        if (tile.HasTile) flags |= 1 << 0;

        if (tile.HasActuator) flags |= 1 << 1;
        if (tile.IsActuated) flags |= 1 << 2;

        if (tile.RedWire) flags |= 1 << 3;
        if (tile.BlueWire) flags |= 1 << 4;
        if (tile.GreenWire) flags |= 1 << 5;
        if (tile.YellowWire) flags |= 1 << 6;

        dst[0] = flags;
        BinaryPrimitives.WriteUInt16LittleEndian(dst[1..], tile.type);
        BinaryPrimitives.WriteInt16LittleEndian(dst[3..], tile.frameX);
        BinaryPrimitives.WriteInt16LittleEndian(dst[5..], tile.frameY);
    }

    public static byte[] GetWiringHash()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> pos = stackalloc byte[8];
        Span<byte> cell = stackalloc byte[TileCellSize];

        for (var y = 0; y < Main.maxTilesY; y++)
        {
            for (var x = 0; x < Main.maxTilesX; x++)
            {
                var tile = Main.tile[x, y];
                if (!Detector.HasWiring(tile)) continue;

                BinaryPrimitives.WriteInt32LittleEndian(pos, x);
                BinaryPrimitives.WriteInt32LittleEndian(pos[4..], y);
                hash.AppendData(pos);

                GetTileHash(cell, tile);
                hash.AppendData(cell);
            }
        }

        return hash.GetHashAndReset();
    }
}
