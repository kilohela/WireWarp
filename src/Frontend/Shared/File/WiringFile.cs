using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.Terraria;

namespace WireWarp.Frontend.Shared.File;

public static class WiringFile
{
    private static string PathName =>
        Path.ChangeExtension(Access.Instance.WorldPathName, ".wwir");

    private static string TempPathName => PathName + ".tmp";

    public static bool Save()
    {
        try
        {
            using var fs = new FileStream(TempPathName, FileMode.Create);
            using var w = new BinaryWriter(fs);
            HeaderFile.Write(w, WiringGraph.Hash.Span);
            WiringSerializer.Serialize(w);
            System.IO.File.Move(TempPathName, PathName, overwrite: true);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"WiringFile.Save failed: {e}");
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            WiringGraph.Clean();

            using var fs = new FileStream(PathName, FileMode.Open);
            using var r = new BinaryReader(fs);
            WiringGraph.SetHash(HeaderFile.Read(r));
            WiringSerializer.Deserialize(r);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"WiringFile.Load failed: {e}");
            WiringGraph.Clean();
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
