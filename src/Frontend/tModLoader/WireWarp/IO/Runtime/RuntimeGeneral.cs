namespace WireWarp.Frontend.tModLoader.IO;

internal static class RuntimeGeneral
{
    private const int MaxMech = 1000;

    private static readonly Dictionary<(int x, int y), long> _mechTime = [];
    private static long _mechTick;

    public static int cannonCoolDown = 0;
    public static int bunnyCannonCoolDown = 0;
    public static int snowballCannonCoolDown = 0;

    public static bool CheckMech(int i, int j, int time)
    {
        if (time <= 0)
            return true;

        if (_mechTime.TryGetValue((i, j), out var expire) && expire > _mechTick)
            return false;

        if (_mechTime.Count >= MaxMech)
            return false;

        _mechTime[(i, j)] = _mechTick + time;
        return true;
    }

    public static void Tick()
    {
        UpdateCannonCoolDown();
        UpdateMechTimer();
    }

    public static void Reset()
    {
        ResetCannonCoolDown();
        ResetMechTimer();
    }

    private static void UpdateCannonCoolDown()
    {
        if (cannonCoolDown > 0)
            cannonCoolDown--;

        if (bunnyCannonCoolDown > 0)
            bunnyCannonCoolDown--;

        if (snowballCannonCoolDown > 0)
            snowballCannonCoolDown--;
    }

    private static void UpdateMechTimer()
    {
        _mechTick++;

        if (_mechTime.Count == 0) return;

        var expired = new List<(int x, int y)>();
        foreach (var (key, expire) in _mechTime)
            if (expire <= _mechTick)
                expired.Add(key);
        expired.ForEach(e => _mechTime.Remove(e));
    }

    private static void ResetCannonCoolDown()
    {
        cannonCoolDown = 0;
        bunnyCannonCoolDown = 0;
        snowballCannonCoolDown = 0;
    }

    private static void ResetMechTimer()
    {
        _mechTick = 0;
        _mechTime.Clear();
    }
}
