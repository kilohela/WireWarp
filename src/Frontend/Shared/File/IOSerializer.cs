using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.File;

public static class IOSerializer
{
    private const uint Magic = 0x4F495757;
    private const uint Version = 1;
    private const int GroupCount = 6;

    public static void Serialize(BinaryWriter w)
    {
        w.BaseStream.Position = 0;

        w.Write(Magic);
        w.Write(Version);
        w.Write(IOGraph.Hash.Span);

        WriteGroups(w);
    }

    public static void Deserialize(BinaryReader r)
    {
        IOGraph.Clean();

        r.BaseStream.Position = 0;

        if (r.ReadUInt32() != Magic) throw new InvalidDataException("IO serializer magic mismatch");
        if (r.ReadUInt32() != Version) throw new InvalidDataException($"IO serializer version mismatch");

        IOGraph.SetHash(r.ReadBytes(32));

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

    private static long ReadTeleporter(BinaryReader r)
    {
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var portId = r.ReadInt32();
            var sx = r.ReadInt32();
            var sy = r.ReadInt32();
            var tx = r.ReadInt32();
            var ty = r.ReadInt32();

            IOExtra.SetTeleporter(portId, ((sx, sy), (tx, ty)));
        }

        return r.BaseStream.Position;
    }

    private static long ReadPumps(BinaryReader r)
    {
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var portId = r.ReadInt32();

            var inletCount = r.ReadInt32();
            var inlets = new List<(int x, int y)>(inletCount);
            for (var j = 0; j < inletCount; j++)
                inlets.Add((r.ReadInt32(), r.ReadInt32()));

            var outletCount = r.ReadInt32();
            var outlets = new List<(int x, int y)>(outletCount);
            for (var j = 0; j < outletCount; j++)
                outlets.Add((r.ReadInt32(), r.ReadInt32()));

            IOExtra.SetPump(portId, inlets, outlets);
        }

        return r.BaseStream.Position;
    }

    private static long ReadWireBulb(BinaryReader r)
    {
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var portId = r.ReadInt32();
            var type = (WireID)r.ReadByte();

            IOExtra.SetWireBulb(portId, type);
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

    private static long WriteTeleporter(BinaryWriter w)
    {
        var start = w.BaseStream.Position;

        var teleporter = IOExtra.Teleporter.OrderBy(kv => kv.Key).ToList();

        w.Write(teleporter.Count);
        foreach (var (portId, (source, target)) in teleporter)
        {
            w.Write(portId);
            w.Write(source.x);
            w.Write(source.y);
            w.Write(target.x);
            w.Write(target.y);
        }

        return start;
    }

    private static long WritePumps(BinaryWriter w)
    {
        var start = w.BaseStream.Position;

        var pumps = IOExtra.Pumps.OrderBy(kv => kv.Key).ToList();

        w.Write(pumps.Count);
        foreach (var (portId, (inlets, outlets)) in pumps)
        {
            w.Write(portId);

            w.Write(inlets.Count);
            foreach (var (x, y) in inlets)
            {
                w.Write(x);
                w.Write(y);
            }

            w.Write(outlets.Count);
            foreach (var (x, y) in outlets)
            {
                w.Write(x);
                w.Write(y);
            }
        }

        return start;
    }

    private static long WriteWireBulb(BinaryWriter w)
    {
        var start = w.BaseStream.Position;

        var wireBulb = IOExtra.WireBulb.OrderBy(kv => kv.Key).ToList();

        w.Write(wireBulb.Count);
        foreach (var (portId, type) in wireBulb)
        {
            w.Write(portId);
            w.Write((byte)type);
        }

        return start;
    }
}
