using System.Diagnostics;
using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Validate
{
    [Conditional("DEBUG")]
    public static void Execute()
    {
        ValidateConstraints();
        ValidateSymmetry();
        ValidateFaultGates();
        ValidateSkipWire();
    }

    private static void ValidateConstraints()
    {
        foreach (var node in WiringGraph.Components.Values)
        {
            switch (node)
            {
                case Input:
                    Debug.Assert((InputID)node.Type != InputID.None,
                        $"{At(node)} Type expect input");
                    Debug.Assert(node.Fanin.Count == 0,
                        $"{At(node)} Fanin expect 0, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is InputPort),
                        $"{At(node)} Fanout expect InputPort");
                    break;

                case InputPort:
                    Debug.Assert((InputID)node.Type != InputID.None,
                        $"{At(node)} Type expect input port");
                    Debug.Assert(node.Fanin.Count == 1,
                        $"{At(node)} Fanin expect 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Input),
                        $"{At(node)} Fanin expect Input");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Wire),
                        $"{At(node)} Fanout expect Wire");
                    break;

                case Output:
                    Debug.Assert((OutputID)node.Type != OutputID.None,
                        $"{At(node)} Type expect output");
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is OutputPort),
                        $"{At(node)} Fanin expect OutputPort");
                    Debug.Assert(node.Fanout.Count == 0,
                        $"{At(node)} Fanout expect 0, got {node.Fanout.Count}");
                    break;

                case OutputPort:
                    Debug.Assert((OutputID)node.Type != OutputID.None,
                        $"{At(node)} Type expect output port");
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Wire),
                        $"{At(node)} Fanin expect Wire");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Output),
                        $"{At(node)} Fanout expect Output");
                    break;

                case Lamp:
                    Debug.Assert((LampID)node.Type != LampID.None,
                        $"{At(node)} Type expect lamp");
                    Debug.Assert(node.Fanin.All(x => x is Wire),
                        $"{At(node)} Fanin expect Wire");
                    Debug.Assert(node.Fanout.Count == 1,
                        $"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Gate),
                        $"{At(node)} Fanout expect Gate");
                    break;

                case Gate:
                    Debug.Assert((GateID)node.Type != GateID.None,
                        $"{At(node)} Type expect gate");
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Lamp),
                        $"{At(node)} Fanin expect Lamp");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Wire),
                        $"{At(node)} Fanout expect Wire");
                    break;

                case Wire:
                    Debug.Assert((WireID)node.Type != WireID.None,
                        $"{At(node)} Type expect wire");
                    Debug.Assert(node.Fanin.Count >= 1,
                        $"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    Debug.Assert(node.Fanin.All(x => x is Gate || x is InputPort),
                        $"{At(node)} Fanin expect Gate or InputPort");
                    Debug.Assert(node.Fanout.Count >= 1,
                        $"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    Debug.Assert(node.Fanout.All(x => x is Lamp || x is OutputPort),
                        $"{At(node)} Fanout expect Lamp or OutputPort");
                    break;
            }
        }
    }

    private static void ValidateFaultGates()
    {
        foreach (var gate in WiringGraph.Gates.Where(g => g.Type == GateID.Fault))
        {
            var faultLamps = gate.Fanin.OfType<Lamp>()
                .Where(l => l.Type == LampID.Fault)
                .ToList();

            Debug.Assert(faultLamps.Count == 1,
                $"{At(gate)} expect exactly 1 fault lamp, got {faultLamps.Count}");
        }
    }

    private static void ValidateSkipWire()
    {
        foreach (var wire in WiringGraph.Wires)
        {
            var inputs = new HashSet<Input>();
            foreach (var pos in wire.Sources)
            {
                Debug.Assert(WiringGraph.InputPos.TryGetValue(pos, out var input),
                    $"{At(wire)} source point ({pos.X},{pos.Y}) not found in InputPos");
                Debug.Assert(inputs.Add(input),
                    $"{At(wire)} input {At(input)} connected by multiple source points");
            }

            var outputs = new HashSet<Output>();
            foreach (var pos in wire.Drains)
            {
                Debug.Assert(WiringGraph.OutputPos.TryGetValue(pos, out var output),
                    $"{At(wire)} drain point ({pos.X},{pos.Y}) not found in OutputPos");
                Debug.Assert(outputs.Add(output),
                    $"{At(wire)} output {At(output)} connected by multiple drain points");
            }
        }
    }

    private static void ValidateSymmetry()
    {
        foreach (var node in WiringGraph.Components.Values)
        {
            foreach (var target in node.Fanout)
                Debug.Assert(target.Fanin.Contains(node),
                    $"{At(node)} edge asymmetry: {At(target)}");

            foreach (var source in node.Fanin)
                Debug.Assert(source.Fanout.Contains(node),
                    $"{At(source)} edge asymmetry: {At(node)}");
        }
    }

    private static string At(IConnectable node) => node switch
    {
        Input i => $"Input:{(InputID)node.Type}#{i.Id}@{i.Origin}",
        InputPort ip => $"InputPort:{(InputID)node.Type}#{ip.Id}",
        Output o => $"Output:{(OutputID)node.Type}#{o.Id}@{o.Origin}",
        OutputPort op => $"OutputPort:{(OutputID)node.Type}#{op.Id}",
        Lamp l => $"Lamp:{(LampID)node.Type}#{l.Id}@{l.Origin}",
        Gate g => $"Gate:{(GateID)node.Type}#{g.Id}@{g.Origin}",
        Wire w => $"Wire:{(WireID)node.Type}#{w.Id}",
        _ => $"#{node.Id}"
    };
}
