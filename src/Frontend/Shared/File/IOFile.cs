using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.File;

public static class IOFile
{
    public static string PathName =>
        Path.ChangeExtension(Access.Instance.WorldPathName, ".wwio");

    private static string TempPathName => PathName + ".tmp";

    public static bool Save()
    {
        try
        {
            using (var fs = new FileStream(TempPathName, FileMode.Create))
            using (var w = new BinaryWriter(fs))
            {
                HeaderFile.Write(w, IOGraph.Hash.Span);
                IOSerializer.Serialize(w);
            }

            System.IO.File.Move(TempPathName, PathName, overwrite: true);
            return true;
        }
        catch (Exception e)
        {
            Access.Instance.Notify($"IOFile.Save failed: {e}");
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
            Access.Instance.Notify($"IOFile.Load failed: {e}");
            IOGraph.Clean();
            return false;
        }
    }
}
