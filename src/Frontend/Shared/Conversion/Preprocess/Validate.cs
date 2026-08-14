using WireWarp.Frontend.Shared.Data;
using WireWarp.Frontend.Shared.ID;

namespace WireWarp.Frontend.Shared.Conversion;

internal static class Validate
{
    public static void Execute()
    {
        Access.Instance.Status("Validating wiring...");
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
                    if ((InputID)node.Type == InputID.None)
                        Access.Instance.Notify($"{At(node)} Type expect input");
                    if (node.Fanin.Count != 0)
                        Access.Instance.Notify($"{At(node)} Fanin expect 0, got {node.Fanin.Count}");
                    if (node.Fanout.Count != 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is InputPort))
                        Access.Instance.Notify($"{At(node)} Fanout expect InputPort");
                    break;

                case InputPort:
                    if ((InputID)node.Type == InputID.None)
                        Access.Instance.Notify($"{At(node)} Type expect input port");
                    if (node.Fanin.Count != 1)
                        Access.Instance.Notify($"{At(node)} Fanin expect 1, got {node.Fanin.Count}");
                    if (!node.Fanin.All(x => x is Input))
                        Access.Instance.Notify($"{At(node)} Fanin expect Input");
                    if (node.Fanout.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is Wire))
                        Access.Instance.Notify($"{At(node)} Fanout expect Wire");
                    break;

                case Output:
                    if ((OutputID)node.Type == OutputID.None)
                        Access.Instance.Notify($"{At(node)} Type expect output");
                    if (node.Fanin.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    if (!node.Fanin.All(x => x is OutputPort))
                        Access.Instance.Notify($"{At(node)} Fanin expect OutputPort");
                    if (node.Fanout.Count != 0)
                        Access.Instance.Notify($"{At(node)} Fanout expect 0, got {node.Fanout.Count}");
                    break;

                case OutputPort:
                    if ((OutputID)node.Type == OutputID.None)
                        Access.Instance.Notify($"{At(node)} Type expect output port");
                    if (node.Fanin.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    if (!node.Fanin.All(x => x is Wire))
                        Access.Instance.Notify($"{At(node)} Fanin expect Wire");
                    if (node.Fanout.Count != 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is Output))
                        Access.Instance.Notify($"{At(node)} Fanout expect Output");
                    break;

                case Lamp:
                    if ((LampID)node.Type == LampID.None)
                        Access.Instance.Notify($"{At(node)} Type expect lamp");
                    if (!node.Fanin.All(x => x is Wire))
                        Access.Instance.Notify($"{At(node)} Fanin expect Wire");
                    if (node.Fanout.Count != 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is Gate))
                        Access.Instance.Notify($"{At(node)} Fanout expect Gate");
                    break;

                case Gate:
                    if ((GateID)node.Type == GateID.None)
                        Access.Instance.Notify($"{At(node)} Type expect gate");
                    if (node.Fanin.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    if (!node.Fanin.All(x => x is Lamp))
                        Access.Instance.Notify($"{At(node)} Fanin expect Lamp");
                    if (node.Fanout.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is Wire))
                        Access.Instance.Notify($"{At(node)} Fanout expect Wire");
                    break;

                case Wire:
                    if ((WireID)node.Type == WireID.None)
                        Access.Instance.Notify($"{At(node)} Type expect wire");
                    if (node.Fanin.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanin expect >= 1, got {node.Fanin.Count}");
                    if (!node.Fanin.All(x => x is Gate || x is InputPort))
                        Access.Instance.Notify($"{At(node)} Fanin expect Gate or InputPort");
                    if (node.Fanout.Count < 1)
                        Access.Instance.Notify($"{At(node)} Fanout expect >= 1, got {node.Fanout.Count}");
                    if (!node.Fanout.All(x => x is Lamp || x is OutputPort))
                        Access.Instance.Notify($"{At(node)} Fanout expect Lamp or OutputPort");
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

            if (faultLamps.Count != 1)
                Access.Instance.Notify($"{At(gate)} expect exactly 1 fault lamp, got {faultLamps.Count}");
        }
    }

    private static void ValidateSkipWire()
    {
        foreach (var wire in WiringGraph.Wires)
        {
            var inputs = new HashSet<Input>();
            foreach (var pos in wire.Sources)
            {
                if (!WiringGraph.InputPos.TryGetValue(pos, out var input))
                    Access.Instance.Notify($"{At(wire)} source point ({pos.X},{pos.Y}) not found in InputPos");
                else if (!inputs.Add(input))
                    Access.Instance.Notify($"{At(wire)} input {At(input)} connected by multiple source points");
            }

            var outputs = new HashSet<Output>();
            foreach (var pos in wire.Drains)
            {
                if (!WiringGraph.OutputPos.TryGetValue(pos, out var output))
                    Access.Instance.Notify($"{At(wire)} drain point ({pos.X},{pos.Y}) not found in OutputPos");
                else if (!outputs.Add(output))
                    Access.Instance.Notify($"{At(wire)} output {At(output)} connected by multiple drain points");
            }
        }
    }

    private static void ValidateSymmetry()
    {
        foreach (var node in WiringGraph.Components.Values)
        {
            foreach (var target in node.Fanout)
                if (!target.Fanin.Contains(node))
                    Access.Instance.Notify($"{At(node)} edge asymmetry: {At(target)}");

            foreach (var source in node.Fanin)
                if (!source.Fanout.Contains(node))
                    Access.Instance.Notify($"{At(source)} edge asymmetry: {At(node)}");
        }
    }

    internal static string At(IConnectable node) => node switch
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
