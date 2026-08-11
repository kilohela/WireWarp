using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.File;

public static partial class IOSerializer
{
    private const int GroupCount = 6;

    public static void Serialize(BinaryWriter w)
    {
        WriteGroups(w);
    }

    public static void Deserialize(BinaryReader r)
    {
        ReadGroups(r);
    }

    private static void ReadGroups(BinaryReader r)
    {
        if (r.ReadInt32() != GroupCount) throw new InvalidDataException("IO serializer group count mismatch");

        var starts = new int[GroupCount];
        for (var i = 0; i < GroupCount; i++)
            starts[i] = r.ReadInt32();

        if (ReadInputs(r) != starts[1]) throw new InvalidDataException("IO serializer group 0 length mismatch");
        if (ReadOutputs(r) != starts[2]) throw new InvalidDataException("IO serializer group 1 length mismatch");
        if (ReadTeleporter(r) != starts[3]) throw new InvalidDataException("IO serializer group 2 length mismatch");
        if (ReadPumps(r) != starts[4]) throw new InvalidDataException("IO serializer group 3 length mismatch");
        if (ReadWireBulb(r) != starts[5]) throw new InvalidDataException("IO serializer group 4 length mismatch");
    }

    private static long ReadInputs(BinaryReader r)
    {
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var x = r.ReadInt32();
            var y = r.ReadInt32();
            var portId = r.ReadInt32();
            var type = (InputID)r.ReadByte();

            IOGraph.SetInput((x, y), portId, type);
        }

        return r.BaseStream.Position;
    }

    private static long ReadOutputs(BinaryReader r)
    {
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var portId = r.ReadInt32();
            var x = r.ReadInt32();
            var y = r.ReadInt32();
            var type = (OutputID)r.ReadByte();

            IOGraph.SetOutput(portId, (x, y), type);
        }

        return r.BaseStream.Position;
    }

    private static void WriteGroups(BinaryWriter w)
    {
        w.Write(GroupCount);

        var groupStartPos = w.BaseStream.Position;
        for (var i = 0; i < GroupCount; i++)
            w.Write(0);

        var starts = new long[GroupCount];

        starts[0] = WriteInputs(w);
        starts[1] = WriteOutputs(w);
        starts[2] = WriteTeleporter(w);
        starts[3] = WritePumps(w);
        starts[4] = WriteWireBulb(w);
        starts[5] = w.BaseStream.Position;

        w.BaseStream.Position = groupStartPos;
        for (var i = 0; i < GroupCount; i++)
            w.Write((uint)starts[i]);
    }

    private static long WriteInputs(BinaryWriter w)
    {
        var start = w.BaseStream.Position;

        var inputs = IOGraph.Inputs.OrderBy(kv => kv.Value.portId).ToList();

        w.Write(inputs.Count);
        foreach (var (pos, (portId, type)) in inputs)
        {
            w.Write(pos.x);
            w.Write(pos.y);
            w.Write(portId);
            w.Write((byte)type);
        }

        return start;
    }

    private static long WriteOutputs(BinaryWriter w)
    {
        var start = w.BaseStream.Position;

        var outputs = IOGraph.Outputs.OrderBy(kv => kv.Key).ToList();

        w.Write(outputs.Count);
        foreach (var (portId, (pos, type)) in outputs)
        {
            w.Write(portId);
            w.Write(pos.x);
            w.Write(pos.y);
            w.Write((byte)type);
        }

        return start;
    }
}
