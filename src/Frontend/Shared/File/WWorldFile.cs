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
            // WorldFile.SaveWorld();

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
                // if (!IOGraph.Hash.Span.SequenceEqual(hash)) throw new InvalidDataException("Header hash mismatch");

                src.CopyTo(dst);
            }

            System.IO.File.Move(WLDTempPathName, Access.Instance.WorldPathName, overwrite: true);

            // WorldFile.LoadWorld();
            return true;
        }
        catch (Exception e)
        {
            Access.Instance.Notify($"WorldFile.Load failed: {e}");
            return false;
        }
    }

    public static bool MatchHash(byte[] hash) => 
        LoadHeader() is { } fileHash && fileHash.AsSpan().SequenceEqual(hash);

    public static byte[]? LoadHeader()
    {
        try
        {
            using var fs = new FileStream(PathName, FileMode.Open);
            using var r = new BinaryReader(fs);
            return HeaderFile.Read(r);
        }
        catch { return null; }
    }
}
