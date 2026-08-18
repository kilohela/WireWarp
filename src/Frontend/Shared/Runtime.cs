using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared;

public static class Runtime
{
    private const int TimeoutWindow = 600;
    public const double FrameTimeoutBudget = 16.67;
    
    private static readonly Stopwatch _frameTimer = Stopwatch.StartNew();

    private static bool _isOpen;
    private static bool _isRun;
    private static long _time;
    private static int _frontendTimeoutCount;
    private static int _backendTimeoutCount;

    public static bool IsOpen => _isOpen;
    public static bool IsRun => _isRun;
    public static long Time => _time;

    public static void Run() { if (_isOpen) _isRun = true; }
    public static void Stop() { if (_isOpen) _isRun = false; }

    public static void Startup()
    {
        if (_isOpen) return;

        UpdateFile();

        if (!Transport.IsOpen)
        {
            Access.Instance.Status("Waiting for backend open...");
            Transport.Open();
        }
        
        Access.Instance.Status("Waiting for backend sync...");
        CheckAck("Backend sync to failed", 
            Transport.SendSyncTo(IOGraph.Hash.ToArray(), WiringFile.PathName));

        Access.Instance.Status("Waiting for backend startup...");
        CheckAck("Backend startup failed", Transport.SendStartup());

        _isOpen = true;
        _isRun = true;
        _time = 0;
        _frontendTimeoutCount = 0;
        _backendTimeoutCount = 0;
        _frameTimer.Restart();

        Access.Instance.Reset();
        IOFrame.Clean();

        Access.Instance.Notify("Frontend started");
    }

    public static void Shutdown()
    {
        _isOpen = false;
        _isRun = false;
        _time = 0;
        _frontendTimeoutCount = 0;
        _backendTimeoutCount = 0;

        Access.Instance.Reset();
        IOFrame.Clean();

        IOGraph.Clean();
        WiringGraph.Clean();

        if (Transport.IsOpen)
        {
            try { CheckAck("Backend shutdown failed", Transport.SendShutdown()); }
            finally { Transport.Close(); }
        }

        Access.Instance.Notify("Frontend shutdown");
    }

    public static void Tick()
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }

        if (_frameTimer.Elapsed.TotalMilliseconds > FrameTimeoutBudget) _frontendTimeoutCount++;
        _frameTimer.Restart();

        if (!_isRun)
        {
            CheckAck("Backend frame failed", 
                Transport.SendFrame(false, _time, []).ack);
        }
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

            if (_time % TimeoutWindow == 0 && (_frontendTimeoutCount > 0 || _backendTimeoutCount > 0))
            {
                Access.Instance.Notify($"Slow frames: frontend {_frontendTimeoutCount}, backend {_backendTimeoutCount} in last {TimeoutWindow} ticks");
                _frontendTimeoutCount = 0;
                _backendTimeoutCount = 0;
            }
        }

        if (_frameTimer.Elapsed.TotalMilliseconds > FrameTimeoutBudget) _backendTimeoutCount++;
        _frameTimer.Restart();
    }

    public static void HitInput(int x, int y, bool hitPoint = true)
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }
        if (!_isRun) { Access.Instance.Notify("Frontend not running"); return; }

        if (IOGraph.Inputs.TryGetValue((x, y), out var input))
        {
            IOFrame.WriteInput(input.portId);
            if (hitPoint)
                Access.Instance.Execute(input.type, input.portId, x, y);
        }
        else
            Access.Instance.Notify($"Point ({x},{y}) not found in Inputs");
    }

    private static void HitOutput(int portId)
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }
        if (!_isRun) { Access.Instance.Notify("Frontend not running"); return; }

        if (IOGraph.Outputs.TryGetValue(portId, out var output))
            Access.Instance.Execute(output.type, portId, output.pos.x, output.pos.y);
        else
            Access.Instance.Notify($"Port ({portId}) not found in Outputs");
    }

    public static void SyncTo()
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }

        Access.Instance.Notify("Saving world...");
        Access.Instance.SaveWorld();
        
        Access.Instance.Notify("Updating wiring files...");
        UpdateFile();

        Access.Instance.Notify("Waiting for backend...");
        CheckAck("Backend sync to failed", 
            Transport.SendSyncTo(IOGraph.Hash.ToArray(), WiringFile.PathName));
        
        Access.Instance.Status("Backend initializing...");
        CheckAck("Backend startup failed", Transport.SendStartup());

        Access.Instance.Notify("Wiring synced to backend");
    }

    public static void SyncFrom()
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }

        Access.Instance.Notify("Waiting for backend...");
        var (ack, payload) = Transport.SendSyncFrom();
        CheckAck("Backend sync from failed", ack);

        if (!payload.hash.SequenceEqual(IOGraph.Hash.Span))
            Access.Instance.Notify("Wiring hash mismatch, sync failed");
        else
        {
            Access.Instance.Notify("Applying wiring state to world...");
            WiringFile.Load();

            IOGraph.Resolve();
            WiringGraph.Resolve();

            WiringGraph.Clean();

            Access.Instance.Notify("Wiring state applied to world");
        }
    }

    public static void Reset()
    {
        if (!_isOpen) { Access.Instance.Notify("Frontend not openning"); return; }

        CheckAck("Backend reset failed", Transport.SendReset());

        _time = 0;
        _frontendTimeoutCount = 0;
        _backendTimeoutCount = 0;
        _frameTimer.Restart();

        Access.Instance.Reset();
        IOFrame.Clean();

        if (!HeaderFile.MatchHash(WWorldFile.PathName, IOGraph.Hash.Span.ToArray()))
            Access.Instance.Notify("World hash mismatch, reset failed");
        else
        {
            Access.Instance.Notify("Loading world snapshot...");
            WWorldFile.Load();

            Access.Instance.Notify("Reloading world...");
            Access.Instance.LoadWorld();
            
            Access.Instance.Notify("Reset complete");
        }
    }

    private static void UpdateFile()
    {
        var sw = Stopwatch.StartNew();

        Access.Instance.Status("Hashing wiring...");
        var hash = Conversion.Hash.Execute();
        Access.Instance.Status($"Hash time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();

        if (!HeaderFile.MatchHash(WiringFile.PathName, hash) || !HeaderFile.MatchHash(IOFile.PathName, hash))
        {
            Access.Instance.Status("Building wiring graph...");
            WiringGraph.Build();
            Access.Instance.Status($"Graph time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
            
            WiringGraph.SetHash(hash);
            
            Access.Instance.Status("Building io graph...");
            IOGraph.Build();
            Access.Instance.Status($"IO time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();

            Access.Instance.Status("Saving wiring graph...");
            WiringFile.Save();
            Access.Instance.Status($"SaveWiring time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
            
            Access.Instance.Status("Saving io graph...");
            IOFile.Save();
            Access.Instance.Status($"SaveIO time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();

            WiringGraph.Clean();
        }
        else
        {
            Access.Instance.Status("Loading io graph...");
            IOFile.Load();
            Access.Instance.Status($"LoadIO time: {sw.Elapsed.TotalSeconds:F2}s"); sw.Restart();
        }

        Access.Instance.Status("Saving world snapshot...");
        WWorldFile.Save();
        Access.Instance.Status($"Snapshot time: {sw.Elapsed.TotalSeconds:F2}s");
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

    private static void CheckAck(string prefix, (int status, string message) ack)
    {
        if (ack.status == 0) return;
        throw new Exception($"{prefix}: {ack.status} {ack.message}");
    }
}
