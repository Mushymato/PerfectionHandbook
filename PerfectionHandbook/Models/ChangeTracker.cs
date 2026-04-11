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
            ModEntry.Log($"{logName}: changed ({newHash})");
            lastHash = newHash;
            return true;
        }
        return false;
    }
}

public sealed class InvalidateTracker(IAssetName assetName) : IChangeTracker
{
    private static readonly Dictionary<IAssetName, List<InvalidateTracker>> invalidateTrackers = [];

    private bool isValid = false;
    public readonly IAssetName AssetName = assetName;

    public static InvalidateTracker GetInvalidateTracker(string name)
    {
        IAssetName assetName = ModEntry.help.GameContent.ParseAssetName(name);
        if (!invalidateTrackers.TryGetValue(assetName, out List<InvalidateTracker>? trackerList))
        {
            trackerList = [];
            invalidateTrackers[assetName] = trackerList;
        }
        InvalidateTracker tracker = new(assetName);
        trackerList.Add(tracker);
        return tracker;
    }

    public bool CheckChanged()
    {
        if (!isValid)
        {
            ModEntry.Log($"{AssetName}: changed ({Game1.ticks})");
            isValid = true;
        }
        return isValid;
    }

    internal static void OnAssetInvalidated(AssetsInvalidatedEventArgs e)
    {
        foreach (IAssetName assetName in e.NamesWithoutLocale)
        {
            if (invalidateTrackers.TryGetValue(assetName, out List<InvalidateTracker>? trackerList))
            {
                foreach (InvalidateTracker tracker in trackerList)
                    tracker.isValid = false;
            }
        }
    }
}
