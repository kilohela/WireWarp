namespace WireWarp.Frontend.Shared.Data;

public class IOFrame(WiringGraph graph)
{
    public int InputPortCount { get; } = graph.InputPorts.Count;
    public int OutputPortCount { get; } = graph.OutputPorts.Count;

    private readonly List<int>[] _inputBuffers = [[], []];
    private readonly List<int>[] _outputBuffers = [[], []];
    private long _tick;

    public void AddInput(int portId) => _inputBuffers[_tick & 1].Add(portId);
    public void AddOutput(int portId) => _outputBuffers[_tick & 1].Add(portId);
    public IReadOnlyList<int> ReadInputs() => _inputBuffers[(_tick + 1) & 1];
    public IReadOnlyList<int> ReadOutputs() => _outputBuffers[(_tick + 1) & 1];

    public void Tick()
    {
        _tick++;
        _inputBuffers[_tick & 1].Clear();
        _outputBuffers[_tick & 1].Clear();
    }

    public void Reset()
    {
        _tick = 0;
        _inputBuffers[0].Clear();
        _outputBuffers[0].Clear();
        _inputBuffers[1].Clear();
        _outputBuffers[1].Clear();
    }
}
