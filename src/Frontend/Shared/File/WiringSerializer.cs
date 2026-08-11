using WireWarp.Frontend.Shared.Data;

namespace WireWarp.Frontend.Shared.File;

public static class WiringSerializer
{
    private const uint Magic = 0xABADBEEF;
    private const uint Version = 1;
    const int GroupCount = 6;

    public static void Serialize(BinaryWriter w)
    {
        w.BaseStream.Position = 0;

        w.Write(Magic);
        w.Write(Version);
        w.Write(WiringGraph.Hash.Span);

        WriteGroups(w);
    }

    private static void WriteGroups(BinaryWriter w)
    {
        w.Write(GroupCount);
        var groupStartPos = w.BaseStream.Position;
        for (var i = 0; i < GroupCount; i++)
            w.Write(0);

        var starts = new long[GroupCount];

        starts[0] = WriteNodes(w, WiringGraph.InputPorts);
        starts[1] = WriteNodes(w, WiringGraph.OutputPorts);
        starts[2] = WriteNodes(w, WiringGraph.Lamps);
        starts[3] = WriteNodes(w, WiringGraph.Gates);
        starts[4] = WriteNodes(w, WiringGraph.Wires);
        starts[5] = w.BaseStream.Position;

        w.BaseStream.Position = groupStartPos;
        for (var i = 0; i < GroupCount; i++)
            w.Write((uint)starts[i]);
    }

    private static long WriteNodes(BinaryWriter w, IReadOnlyList<IConnectable> nodes)
    {
        var start = w.BaseStream.Position;

        w.Write(nodes.Count);
        foreach (var node in nodes)
            WriteNode(w, node);

        return start;
    }

    private static void WriteNode(BinaryWriter w, IConnectable node)
    {
        w.Write(node.Type);
        w.Write(node.Id);

        if (node is InputPort ip) w.Write(ip.PortId);
        if (node is OutputPort op) w.Write(op.PortId);

        var fanoutIds = node.Fanout.Select(n => n.Id).OrderBy(id => id).ToList();
        w.Write(fanoutIds.Count);
        foreach (var id in fanoutIds)
            w.Write(id);
    }
}
