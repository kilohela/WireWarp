using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.File;

public static class IOFile
{
    private static string PathName =>
        Path.ChangeExtension(Access.Instance.WorldPathName, ".wwio");

    private static string TempPathName => PathName + ".tmp";

    public static bool Save()
    {
        try
        {
            using var fs = new FileStream(TempPathName, FileMode.Create);
            using var w = new BinaryWriter(fs);
            HeaderFile.Write(w, IOGraph.Hash.Span);
            IOSerializer.Serialize(w);
            System.IO.File.Move(TempPathName, PathName, overwrite: true);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"IOFile.Save failed: {e}");
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            IOGraph.Clean();

            using var fs = new FileStream(PathName, FileMode.Open);
            using var r = new BinaryReader(fs);
            IOGraph.SetHash(HeaderFile.Read(r));
            IOSerializer.Deserialize(r);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"IOFile.Load failed: {e}");
            IOGraph.Clean();
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
