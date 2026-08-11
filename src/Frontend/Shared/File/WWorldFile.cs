using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.Terraria;
using WireWarp.Frontend.Shared.Terraria.IO;

namespace WireWarp.Frontend.Shared.File;

public static class WWorldFile
{
    private static string PathName =>
        Path.ChangeExtension(Main.worldPathName, ".wwld");

    private static string WWLDTempPathName => PathName + ".tmp";
    private static string WLDTempPathName => Main.worldPathName + ".tmp";

    public static bool Save()
    {
        try
        {
            // WorldFile.SaveWorld();

            using (var src = new FileStream(Main.worldPathName, FileMode.Open))
            using (var dst = new FileStream(WWLDTempPathName, FileMode.Create))
            using (var w = new BinaryWriter(dst))
            {
                HeaderFile.Write(w, IOGraph.Hash.Span);
                src.CopyTo(dst);
            }

            System.IO.File.Move(WWLDTempPathName, PathName, overwrite: true);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"WorldFile.Save failed: {e}");
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            using (var src = new FileStream(PathName, FileMode.Open))
            using (var dst = new FileStream(WLDTempPathName, FileMode.Create))
            using (var r = new BinaryReader(src))
            {
                var hash = HeaderFile.Read(r);
                if (!IOGraph.Hash.Span.SequenceEqual(hash)) throw new InvalidDataException("Header hash mismatch");

                src.CopyTo(dst);
            }

            System.IO.File.Move(WLDTempPathName, Main.worldPathName, overwrite: true);

            // WorldFile.LoadWorld();
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"WorldFile.Load failed: {e}");
            return false;
        }
    }
}
