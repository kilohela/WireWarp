using System.Diagnostics;
using System.IO.Pipes;

namespace WireWarp.Frontend.Shared;

public static class Transport
{
    private const uint Magic = 0xBADBEEF;
    private const ushort Version = 1;
    private const string PipeName = "WireWarp";

    private static NamedPipeServerStream? _pipe;
    private static long _sendId;
    private static long _lastId;

    public static double FrameTimeoutBudget { get; set; } = 16.67;
    public static int FrameTimeoutCount { get; set; }

    public static bool IsOpen => _pipe?.IsConnected ?? false;

    public static void Open()
    {
        _pipe = new NamedPipeServerStream(
            PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        _pipe.WaitForConnection();
    }

    public static void Close()
    {
        _pipe?.Dispose();
        _pipe = null;
    }

    private enum Tag : ushort
    {
        Startup = 1, StartupAck = 2,
        SyncTo = 3, SyncToAck = 4,
        SyncFrom = 5, SyncFromAck = 6,
        Reset = 7, ResetAck = 8,
        Frame = 9, FrameAck = 10,
        Shutdown = 11, ShutdownAck = 12,
    }

    public static (int status, string message) SendStartup() => 
        UnpackAck(Request(Tag.Startup, [])).ack;

    public static (int status, string message) SendSyncTo(byte[] hash, string path)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write(hash);
        w.Write(path);
        return UnpackAck(Request(Tag.SyncTo, ms.ToArray())).ack;
    }

    public static ((int status, string message) ack, (byte[] hash, string path) payload) SendSyncFrom()
    {
        var (ack, payload) = UnpackAck(Request(Tag.SyncFrom, []));
        if (ack.status == 0)
        {
            using var ms = new MemoryStream(payload);
            using var r = new BinaryReader(ms);
            return (ack, (r.ReadBytes(32), r.ReadString()));
        }
        else
            return (ack, ([], ""));
    }

    public static (int status, string message) SendReset() => 
        UnpackAck(Request(Tag.Reset, [])).ack;

    public static (int status, string message) SendShutdown() => 
        UnpackAck(Request(Tag.Shutdown, [])).ack;

    public static ((int status, string message) ack, IReadOnlyList<(int portId, int count)> payload) SendFrame(
        bool run, long tick, IReadOnlyList<(int portId, int count)> inputs)
    {
        var sw = Stopwatch.StartNew();
        var body = Request(Tag.Frame, PackFrame(run, tick, inputs));
        sw.Stop();

        if (sw.ElapsedMilliseconds > FrameTimeoutBudget) FrameTimeoutCount++;

        var (ack, payload) = UnpackAck(body);
        if (ack.status == 0)
            return (ack, UnpackFrameAck(payload));
        else
            return (ack, []);
    }

    private static byte[] Request(Tag tag, byte[] body)
    {
        if (!IsOpen) throw new InvalidOperationException("Transport not open");

        WriteMessage(tag, body);
        var (respTag, id, respBody) = ReadMessage();

        if (_lastId != 0 && id != _lastId + 1)
            throw new InvalidDataException($"Message gap detected: expected {_lastId + 1}, got {id}");
        _lastId = id;

        var expected = (Tag)((ushort)tag + 1);
        if (respTag != expected)
            throw new InvalidDataException($"Unexpected tag {(ushort)respTag}, expected {(ushort)expected}");

        return respBody;
    }

    private static void WriteMessage(Tag tag, byte[] body)
    {
        var id = Interlocked.Increment(ref _sendId);

        using var ms = new MemoryStream(20 + body.Length);
        using var w = new BinaryWriter(ms);
        
        w.Write(Magic);
        w.Write(Version);
        w.Write((ushort)tag);
        w.Write(id);

        w.Write(body.Length);
        w.Write(body);

        _pipe!.Write(ms.GetBuffer(), 0, (int)ms.Length);
    }

    private static (Tag tag, long messageId, byte[] body) ReadMessage()
    {
        var header = new byte[20];
        _pipe!.ReadExactly(header);

        uint magic; ushort version; ushort tag; long id; int length;

        using var ms = new MemoryStream(header);
        using var r = new BinaryReader(ms);
        
        magic = r.ReadUInt32();
        version = r.ReadUInt16();
        tag = r.ReadUInt16();
        id = r.ReadInt64();
        length = r.ReadInt32();

        if (magic != Magic) throw new InvalidDataException("Header magic mismatch");
        if (version != Version) throw new InvalidDataException("Header version mismatch");
        
        var body = new byte[length];
        _pipe.ReadExactly(body);
        return ((Tag)tag, id, body);
    }

    private static ((int status, string message) ack, byte[] payload) UnpackAck(byte[] body)
    {
        using var ms = new MemoryStream(body);
        using var r = new BinaryReader(ms);

        var status = r.ReadInt32();
        var message = r.ReadString();
        var payload = r.ReadBytes((int)(ms.Length - ms.Position));

        return ((status, message), payload);
    }

    private static byte[] PackFrame(bool run, long tick, IReadOnlyList<(int portId, int count)> inputs)
    {
        using var ms = new MemoryStream(13 + 8 * inputs.Count);
        using var w = new BinaryWriter(ms);

        w.Write(run);
        w.Write(tick);

        w.Write(inputs.Count);
        foreach (var (portId, count) in inputs)
        {
            w.Write(portId);
            w.Write(count);
        }

        return ms.ToArray();
    }

    private static List<(int portId, int count)> UnpackFrameAck(byte[] body)
    {
        using var ms = new MemoryStream(body);
        using var r = new BinaryReader(ms);

        var count = r.ReadInt32();
        var result = new List<(int, int)>(count);
        for (var i = 0; i < count; i++)
            result.Add((r.ReadInt32(), r.ReadInt32()));

        return result;
    }
}
