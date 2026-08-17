namespace WireWarp.Backend.CSharp.File;

public static class WiringFile
{
    private enum LampID : byte
    {
        None = 0,
        On,
        Off,
        Fault,
    }

    private enum GateID : byte
    {
        None = 0,
        AND,
        NAND,
        OR,
        NOR,
        XOR,
        XNOR,
        Fault,
    }
}
