using Terraria;
using Terraria.GameContent.Events;

using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.tModLoader.IO;

partial class RuntimeOutput
{
    private static void PartyMonolith(IOGraph iOGraph, int i, int j)
        => BirthdayParty.ToggleManualParty();
}
