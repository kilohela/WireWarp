using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;

namespace WireWarp.Frontend.Shared;

public static class Runtime
{
    private const int FrameTimeoutWindow = 600;
    private const double FrameTimeoutBudget = 16.67;

    private static readonly Stopwatch _frameTimer = Stopwatch.StartNew();
    private static readonly FrameStats _frameStats = new();
    
    private static readonly byte[] _hash = new byte[32];
    public static ReadOnlyMemory<byte> Hash => _hash;

    private static bool _isOpen;
    private static bool _isRun;
    private static long _time;

    public static bool IsOpen => _isOpen;
    public static bool IsRun => _isRun;
    public static long Time => _time;

    public static void Run()
    {
        if (!_isOpen) throw new Exception("Frontend not open.");
        else _isRun = true;
    }

    public static void Stop()
    {
        if (!_isOpen) throw new Exception("Frontend not open.");
        else _isRun = false;
    }

    public static void Startup()
    {
        if (_isOpen) return;

        var sw = Stopwatch.StartNew();
        try
        {
            Access.Instance.Status("Updating files...");
            UpdateFile();

            Access.Instance.Status("Waiting for backend open...");
            Transport.Open();

            Access.Instance.Status("Waiting for backend sync...");
            CheckAck("Backend sync to failed", Transport.SendSyncTo(_hash, WiringFile.PathName));

            Access.Instance.Status("Waiting for backend startup...");
            CheckAck("Backend startup failed", Transport.SendStartup());

            _isOpen = true;
            _isRun = true;

            _time = 0;

            _frameStats.Reset();
            _frameTimer.Restart();

            Access.Instance.Reset();
            IOFrame.Clean();
        }
        catch
        { try { Shutdown(); } catch { } throw; }
        
        Access.Instance.Notify($"Frontend started in {sw.Elapsed.TotalSeconds:F2}s");
    }

    public static void Shutdown()
    {
        _isOpen = false;
        _isRun = false;

        _time = 0;

        _frameStats.Reset();
        
        Array.Clear(_hash);

        Access.Instance.Reset();
        IOFrame.Clean();

        IOGraph.Clean();
        WiringGraph.Clean();

        try { CheckAck("Backend shutdown failed", Transport.SendShutdown()); }
        finally { Transport.Close(); }

        Access.Instance.Notify("Frontend shutdown");
    }

    public static void Tick()
    {
        if (!_isOpen) return;

        var other = _frameTimer.Elapsed.TotalMilliseconds;
        _frameTimer.Restart();

        try
        {
            var (ack, outputs) = Transport.CompleteFrame();
            var backend = Math.Max(0, Transport.LatencyTime);

            CheckAck("Backend frame failed", ack);

            if (_isRun)
            {
                foreach (var output in UnPackRLE(outputs)) HitOutput(output);
                Transport.SendFrameAsync(true, _time, PackRLE(IOFrame.ReadInputs()));

                Access.Instance.Tick();
                IOFrame.Tick();

                _time++;
            }
            else
            {
                Transport.SendFrameAsync(false, _time, []);
            }

            var frontend = _frameTimer.Elapsed.TotalMilliseconds;
            _frameStats.Record(frontend, backend, other);

            if (_isRun && _time % FrameTimeoutWindow == 0)
            {
                if (_frameStats.HasTimeouts)
                    Access.Instance.Notify(_frameStats.Report());
                else
                    _frameStats.Reset();
            }
        }
        catch
        { try { Shutdown(); } catch { } throw; }

        _frameTimer.Restart();
    }

    public static bool HitInput(int x, int y, bool hitPoint = true)
    {
        if (!_isOpen || !_isRun) return false;

        if (IOGraph.Inputs.TryGetValue((x, y), out var input))
        {
            if (hitPoint) Access.Instance.Execute(input.type, input.portId, x, y);
            IOFrame.WriteInput(input.portId);
            return true;
        }

        Access.Instance.Notify($"Point ({x},{y}) not found in Inputs");
        return false;
    }

    private static bool HitOutput(int portId)
    {
        if (!_isOpen || !_isRun) return false;

        if (IOGraph.Outputs.TryGetValue(portId, out var output))
        {
            Access.Instance.Execute(output.type, portId, output.pos.x, output.pos.y);
            return true;
        }

        Access.Instance.Notify($"Port ({portId}) not found in Outputs");
        return false;
    }

    public static void SyncTo()
    {
        if (!_isOpen) throw new Exception("Frontend not open.");

        var sw = Stopwatch.StartNew();

        try
        {
            Access.Instance.Notify("Saving world...");
            try { Access.Instance.SaveWorld(); }
            catch (Exception e) { throw new Exception($"Failed to save world: {e.Message}", e); }

            Access.Instance.Notify("Updating wiring files...");
            UpdateFile();

            Access.Instance.Notify("Waiting for backend...");
            CheckAck("Backend sync to failed", Transport.SendSyncTo(_hash, WiringFile.PathName));

            Access.Instance.Status("Backend initializing...");
            CheckAck("Backend startup failed", Transport.SendStartup());
        }
        catch
        {
            try { Shutdown(); } catch { }
            throw;
        }

        Access.Instance.Notify($"Wiring synced to backend in {sw.Elapsed.TotalSeconds:F2}s");
    }

    public static void SyncFrom()
    {
        if (!_isOpen) throw new Exception("Frontend not open.");

        var sw = Stopwatch.StartNew();
        try
        {
            Access.Instance.Notify("Waiting for backend...");
            var (ack, payload) = Transport.SendSyncFrom();
            CheckAck("Backend sync from failed", ack);

            if (!payload.hash.AsSpan().SequenceEqual(_hash))
                throw new Exception("Wiring hash mismatch, sync failed.");

            Access.Instance.Notify("Applying wiring state to world...");
            WiringFile.Load();

            IOGraph.Resolve();
            WiringGraph.Resolve();
        }
        catch
        {
            { try { Shutdown(); } catch { } throw; }
            throw;
        }

        Access.Instance.Notify($"Wiring state applied to world in {sw.Elapsed.TotalSeconds:F2}s");
    }

    public static void Reset()
    {
        if (!_isOpen) throw new Exception("Frontend not open.");

        var sw = Stopwatch.StartNew();
        try
        {
            CheckAck("Backend reset failed", Transport.SendReset());

            _time = 0;
            _frameStats.Reset();
            _frameTimer.Restart();

            Access.Instance.Reset();
            IOFrame.Clean();

            if (!HeaderFile.MatchHash(WWorldFile.PathName, _hash))
                throw new Exception("World snapshot hash mismatch, reset aborted.");

            Access.Instance.Notify("Loading world snapshot...");
            WWorldFile.Load();

            Access.Instance.Notify("Reloading world...");
            try { Access.Instance.LoadWorld(); }
            catch (Exception e) { throw new Exception($"Failed to reload world: {e.Message}", e);  }
        }
        catch
        {
            { try { Shutdown(); } catch { } throw; }
            throw;
        }

        Access.Instance.Notify($"Reset complete in {sw.Elapsed.TotalSeconds:F2}s");
    }

    public static void Report()
    {
        ReportFile.Write();
        Access.Instance.Notify($"Report written to {ReportFile.PathName}");
    }

    private static void UpdateFile()
    {
        var sw = Stopwatch.StartNew();
        byte[] hash = [];

        try
        {
            Access.Instance.Status("Hashing wiring...");
            hash = Conversion.Hash.Execute();
            var hashTime = sw.Elapsed.TotalMilliseconds;
            sw.Restart();

            Data.Report.SetWorldPath(Access.Instance.WorldPathName);

            var built = false;
            if (!HeaderFile.MatchHash(WiringFile.PathName, hash) ||
                !HeaderFile.MatchHash(IOFile.PathName, hash))
            {
                built = true;

                Access.Instance.Status("Building wiring graph...");
                WiringGraph.Build();
                
                Data.Report.Stages.Insert(0, ("Hash", hashTime));

                Data.Report.SetHash(hash);
                WiringGraph.SetHash(hash);

                Access.Instance.Status("Building io graph...");
                IOGraph.Build();
                Data.Report.AddStage("BuildIO", sw.Elapsed.TotalMilliseconds); sw.Restart();

                Access.Instance.Status("Saving wiring graph...");
                WiringFile.Save();
                Data.Report.AddStage("SaveWiring", sw.Elapsed.TotalMilliseconds); sw.Restart();

                Access.Instance.Status("Saving io graph...");
                IOFile.Save();
                Data.Report.AddStage("SaveIO", sw.Elapsed.TotalMilliseconds); sw.Restart();

                WiringGraph.Clean();
            }
            else
            {
                Access.Instance.Status("Loading io graph...");
                IOFile.Load();
            }

            hash.CopyTo(_hash);

            Access.Instance.Status("Saving world snapshot...");
            WWorldFile.Save();

            if (built)
            {
                Data.Report.AddStage("Snapshot", sw.Elapsed.TotalMilliseconds);
                Data.Report.Success = Data.Report.Errors.Count == 0;
            }
        }
        catch (Exception e)
        {
            Data.Report.Success = false;
            if (hash.Length > 0) Data.Report.SetHash(hash);
            Data.Report.Errors.Add($"{e.GetType().Name}: {e.Message}");

            try { Report(); }
            catch (Exception reportException)
            { Access.Instance.Notify($"Report write failed: {reportException.Message}"); }

            throw;
        }
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

    private sealed class FrameStats
    {
        private int _count;
        private int _timeouts;

        private double _fSum, _fMax;
        private double _bSum, _bMax;
        private double _oSum, _oMax;
        private double _tSum, _tMax;

        public bool HasTimeouts => _timeouts > 0;

        public void Record(double frontend, double backend, double other)
        {
            var total = frontend + other;

            _count++;
            if (total > FrameTimeoutBudget) _timeouts++;

            _fSum += frontend; _fMax = Math.Max(_fMax, frontend);
            _bSum += backend; _bMax = Math.Max(_bMax, backend);
            _oSum += other; _oMax = Math.Max(_oMax, other);
            _tSum += total; _tMax = Math.Max(_tMax, total);
        }

        public string Report()
        {
            var line = $"Slow frames: F:{_fSum / _count:F2}/{_fMax:F2}ms, " +
                       $"B:{_bSum / _count:F2}/{_bMax:F2}ms, " +
                       $"O:{_oSum / _count:F2}/{_oMax:F2}ms, " +
                       $"T:{_tSum / _count:F2}/{_tMax:F2}ms";
            Reset();
            return line;
        }

        public void Reset()
        {
            _count = 0;
            _timeouts = 0;
            _fSum = _fMax = _bSum = _bMax = _oSum = _oMax = _tSum = _tMax = 0;
        }
    }
}
