namespace WireWarp.Frontend.Shared.Terraria;

internal static class Main
{
    internal static int maxTilesX => Access.Instance.MaxTilesX;
    internal static int maxTilesY => Access.Instance.MaxTilesY;
    internal static string worldPathName => Access.Instance.WorldPathName;

    internal static TileArray tile { get; } = new();
}

internal sealed class TileArray
{
    internal Tile this[int x, int y] => Access.Instance.Tile(x, y);
}
