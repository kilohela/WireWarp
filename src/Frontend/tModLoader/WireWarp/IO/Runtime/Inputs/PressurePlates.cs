using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeInput
{
    private static void PressurePlates(IOGraph iOGraph, int i, int j)
    {
        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
    }
}
