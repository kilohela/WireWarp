using Terraria;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void Lampposts(int i, int j)
        => Wiring.ToggleLampPost(i, j, Main.tile[i, j], null, false);
}
