using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeInput
{
    private static void Timers(int i, int j, int portId)
    {
        if (Main.tile[i, j].TileFrameY == 0)
        {
            Main.tile[i, j].TileFrameY = 18;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // CheckMech(i, j, 18000);
            }
        }
        else
        {
            Main.tile[i, j].TileFrameY = 0;
        }

        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
    }
}
