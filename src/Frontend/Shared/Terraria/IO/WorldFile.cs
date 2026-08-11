namespace WireWarp.Frontend.Shared.Terraria.IO;

public static class WorldFile
{
    public static void SaveWorld(bool resetTime = false, bool useTemps = false, bool canBeSkipped = false) =>
        Access.Instance.SaveWorld(resetTime, useTemps, canBeSkipped);

    public static void LoadWorld() => Access.Instance.LoadWorld();
}
