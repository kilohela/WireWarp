namespace WireWarp.Frontend.Shared.Data;

public static class IOFrame
{
    private static readonly List<int>[] _inputBuffers = [[], []];
    private static readonly List<int>[] _outputBuffers = [[], []];
    private static long _tick;

    public static void AddInput(int portId) => _inputBuffers[_tick & 1].Add(portId);
    public static void AddOutput(int portId) => _outputBuffers[_tick & 1].Add(portId);
    public static IReadOnlyList<int> ReadInputs() => _inputBuffers[(_tick + 1) & 1];
    public static IReadOnlyList<int> ReadOutputs() => _outputBuffers[(_tick + 1) & 1];

    public static void Tick()
    {
        _tick++;
        _inputBuffers[_tick & 1].Clear();
        _outputBuffers[_tick & 1].Clear();
    }

    public static void Reset()
    {
        _tick = 0;
        _inputBuffers[0].Clear();
        _outputBuffers[0].Clear();
        _inputBuffers[1].Clear();
        _outputBuffers[1].Clear();
    }
}
