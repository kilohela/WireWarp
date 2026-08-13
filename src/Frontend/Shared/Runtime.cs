using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared;

public static class Runtime
{
    private static bool _isRun;
    private static long _time;

    public static bool IsRun => _isRun;
    public static long Time => _time;

    public static void Run() => _isRun = true;
    public static void Stop() => _isRun = false;

    public static void Startup()
    {
        UpdateFile();

        if (!Transport.IsOpen)
        {
            Transport.Open();
            CheckAck("Backend startup failed", Transport.SendStartup());
        }

        _isRun = true;
        _time = 0;

        Access.Instance.Reset();
        IOFrame.Clean();
    }

    public static void Shutdown()
    {
        if (Transport.IsOpen)
        {
            CheckAck("Backend shutdown failed", Transport.SendShutdown(), false);
            Transport.Close();
        }

        _isRun = false;
        _time = 0;

        Access.Instance.Reset();
        IOFrame.Clean();

        IOGraph.Clean();
        WiringGraph.Clean();
    }

    public static void Tick()
    {
        if (!_isRun)
            CheckAck("Backend frame failed", 
                Transport.SendFrame(false, _time, []).ack);
        else
        {
            foreach (var output in IOFrame.ReadOutputs())
                HitOutput(output);

            var inputs = PackRLE(IOFrame.ReadInputs());
            
            var (ack, outputs) = Transport.SendFrame(true, _time, inputs);
            CheckAck("Backend frame failed", ack);

            foreach (var output in UnPackRLE(outputs))
                IOFrame.WriteOutput(output);

            Access.Instance.Tick();
            IOFrame.Tick();

            _time++;
        }
    }

    public static void HitInput(int x, int y, bool hitPoint = true)
    {
        if (!_isRun) return;

        if (IOGraph.Inputs.TryGetValue((x, y), out var input))
        {
            IOFrame.WriteInput(input.portId);
            if (hitPoint)
                Access.Instance.Execute(input.type, input.portId, x, y);
        }
        else
            Debug.WriteLine($"Point ({x},{y}) not found in Inputs");
    }

    private static void HitOutput(int portId)
    {
        if (!_isRun) return;

        if (IOGraph.Outputs.TryGetValue(portId, out var output))
            Access.Instance.Execute(output.type, portId, output.pos.x, output.pos.y);
        else
            Debug.WriteLine($"Port ({portId}) not found in Outputs");
    }

    public static void SyncTo()
    {
        Access.Instance.SaveWorld();
        UpdateFile();
        CheckAck("Backend sync to failed", 
            Transport.SendSyncTo(IOGraph.Hash.ToArray(), WiringFile.PathName));
    }

    public static void SyncFrom()
    {
        var (ack, payload) = Transport.SendSyncFrom();
        CheckAck("Backend sync from failed", ack);

        if (!payload.hash.SequenceEqual(IOGraph.Hash.Span))
            Debug.WriteLine("Hash not match, sync failed");
        else
        {
            WiringFile.Load();

            IOGraph.Resolve();
            WiringGraph.Resolve();

            WiringGraph.Clean(); 
        }
    }

    public static void Reset()
    {
        CheckAck("Backend reset failed", Transport.SendReset());

        _time = 0;

        Access.Instance.Reset();
        IOFrame.Clean();

        if (!WWorldFile.MatchHash(IOGraph.Hash.Span.ToArray()))
            Debug.WriteLine($"Hash not match, reset failed");
        else
        {
            WWorldFile.Load();
            Access.Instance.LoadWorld();
        }
    }

    private static void UpdateFile()
    {
        var hash = WiringHash.GetHash();

        if (!WiringFile.MatchHash(hash) || !IOFile.MatchHash(hash))
        {
            WiringGraph.SetHash(hash);
            WiringGraph.Build();
            IOGraph.Build();

            WiringFile.Save();
            IOFile.Save();

            WiringGraph.Clean();
        }
        else
        {
            IOFile.Load();
        }

        WWorldFile.Save();
    }

    private static List<(int portId, int count)> PackRLE(IReadOnlyList<int> ids)
    {
        var result = new List<(int portId, int count)>();
        foreach (var id in ids)
        {
            if (result.Count > 0 && result[^1].portId == id)
                result[^1] = (id, result[^1].count + 1);
            else
                result.Add((id, 1));
        }
        return result;
    }

    private static IEnumerable<int> UnPackRLE(IReadOnlyList<(int portId, int count)> ids)
    {
        foreach (var (portId, count) in ids)
            for (var k = 0; k < count; k++)
                yield return portId;
    }

    public static void CheckAck(string prefix, (int status, string message) ack, bool @throw = true)
    {
        if (ack.status == 0) return;
        if (@throw) throw new Exception($"{prefix}: {ack.status} {ack.message}");
        Debug.WriteLine($"{prefix}: {ack.status} {ack.message}");
    }
}
