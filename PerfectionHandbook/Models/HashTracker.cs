namespace PerfectionHandbook.Models;

public sealed class HashTracker(string logName, Func<int> getHash)
{
    private int lastHash = -1;

    public bool CheckHashChanged()
    {
        int newHash = getHash();
        if (newHash != lastHash)
        {
            ModEntry.Log($"{logName}: {lastHash} -> {newHash}");
            lastHash = newHash;
            return true;
        }
        return false;
    }
}
