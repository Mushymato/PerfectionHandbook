using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerfectionHandbook.Models;

public interface IChangeTracker
{
    bool CheckChanged();
}

public sealed class HashTracker(string logName, Func<int> getHash) : IChangeTracker
{
    private int lastHash = -1;

    public bool CheckChanged()
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

public sealed class InvalidateTracker(IAssetName assetName) : IChangeTracker
{
    private static readonly Dictionary<IAssetName, List<InvalidateTracker>> invalidateTrackers = [];

    private int invalidatedTick = -1;
    public readonly IAssetName AssetName = assetName;

    public static InvalidateTracker GetInvalidateTracker(string name)
    {
        IAssetName assetName = ModEntry.help.GameContent.ParseAssetName(name);
        if (!invalidateTrackers.TryGetValue(assetName, out List<InvalidateTracker>? trackerList))
        {
            ModEntry.Log($"MISS: {assetName}");
            trackerList = [];
            invalidateTrackers[assetName] = trackerList;
        }
        else
        {
            ModEntry.Log($"HIT: {assetName}");
        }
        InvalidateTracker tracker = new(assetName);
        trackerList.Add(tracker);
        return tracker;
    }

    public bool CheckChanged()
    {
        if (invalidatedTick != Game1.ticks)
        {
            ModEntry.Log($"{AssetName}: changed");
            Game1.ticks = invalidatedTick;
            return true;
        }
        return false;
    }

    internal static void OnAssetInvalidated(AssetsInvalidatedEventArgs e)
    {
        foreach (IAssetName assetName in e.NamesWithoutLocale)
        {
            if (invalidateTrackers.TryGetValue(assetName, out List<InvalidateTracker>? trackerList))
            {
                ModEntry.Log($"MARK: {assetName}");
                foreach (InvalidateTracker tracker in trackerList)
                    tracker.invalidatedTick = Game1.ticks;
            }
        }
    }
}
