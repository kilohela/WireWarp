using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.File;
using WireWarp.Frontend.Shared.Terraria.IO;

namespace WireWarp.Frontend.Shared;

public static class Runtime
{
    private static bool _run;
    private static long _time;

    public static bool Run => _run;
    public static long Time => _time;

    public static void Play() => _run = true;
    public static void Stop() => _run = false;

    public static void Start()
    {
        SyncFile();

        Access.Instance.Reset();
        IOFrame.Clean();

        _run = true;
        _time = 0;
    }

    public static void End()
    {
        _run = false;

        Access.Instance.Reset();
        IOFrame.Clean();
        IOGraph.Clean();
        WiringGraph.Clean();
    }

    public static void Tick()
    {
        if (!_run) return;

        foreach (var output in IOFrame.ReadOutputs())
            HitOutput(output);

        // TODO: IPC sync wirte outputs and read inputs

        Access.Instance.Tick();
        IOFrame.Tick();

        _time++;
    }

    public static void HitInput(int x, int y, bool hitPoint = true)
    {
        if (!_run) return;

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
        if (!_run) return;

        if (IOGraph.Outputs.TryGetValue(portId, out var output))
            Access.Instance.Execute(output.type, portId, output.pos.x, output.pos.y);
        else
            Debug.WriteLine($"Port ({portId}) not found in Outputs");
    }

    public static void SyncTo()
    {
        WorldFile.SaveWorld();
        Start();
    }

    public static void SyncFrom()
    {
        Reset();
        // TODO: IPC sync tile state from backend
    }

    public static void Reset()
    {
        _time = 0;

        Access.Instance.Reset();
        IOFrame.Clean();

        if (WWorldFile.MatchHash(IOGraph.Hash.Span.ToArray()))
        {
            WWorldFile.Load();
            WorldFile.LoadWorld();
        }
        else
            Debug.WriteLine($"Hash not match, reset failed");
    }

    private static void SyncFile()
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

        // TODO: IPC sync file to backend
    }
}
