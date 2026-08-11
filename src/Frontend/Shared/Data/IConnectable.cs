namespace WireWarp.Frontend.Shared.Data;

public interface IConnectable
{
    int Id { get; }

    byte Type { get; }
    
    HashSet<IConnectable> Fanin { get; }
    HashSet<IConnectable> Fanout { get; }
}