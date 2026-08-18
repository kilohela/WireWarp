using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.File;

public static class WWorldFile
{
    public static string PathName =>
        Path.ChangeExtension(Access.Instance.WorldPathName, ".wwld");

    private static string WWLDTempPathName => PathName + ".tmp";
    private static string WLDTempPathName => Access.Instance.WorldPathName + ".tmp";

    public static bool Save()
    {
        try
        {
            using (var src = new FileStream(Access.Instance.WorldPathName, FileMode.Open))
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
            Access.Instance.Notify($"WorldFile.Save failed: {e}");
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
                src.CopyTo(dst);
            }

            System.IO.File.Move(WLDTempPathName, Access.Instance.WorldPathName, overwrite: true);

            return true;
        }
        catch (Exception e)
        {
            Access.Instance.Notify($"WorldFile.Load failed: {e}");
            return false;
        }
    }
}
