using System.Diagnostics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.Models;

public sealed record EventInfo(string EventId, string[] Preconditions, string Script, string[] Characters);

public sealed record LocationInfo(string LocationId, GameLocation Location)
{
    public LocationData? Data { get; private set; }
    public IReadOnlyDictionary<string, SpawnFishData>? Fishes { get; private set; }

    public void ReloadLocationData()
    {
        Data = Location.GetData();
        if (Data == null)
            return;

        // fish
        Dictionary<string, SpawnFishData> fishes = [];
        foreach (SpawnFishData spawnFishData in Data.Fish ?? [])
        {
            FishAreaData? fishAreaData = null;
            if (spawnFishData.Id != null)
            {
                Data.FishAreas.TryGetValue(spawnFishData.Id, out fishAreaData);
            }
            foreach (ParsedItemData parsedItemData in GameQueryHelper.SimplifiedResolveAll(spawnFishData, Location))
            {
                fishes[parsedItemData.QualifiedItemId] = spawnFishData;
            }
        }
        Fishes = fishes;
    }

    public void ReloadEventData() { }
}

public static class LocationInfoCache
{
    private static readonly HashTracker hashLocations = new(
        nameof(Game1.locations),
        static () => Game1.locations.GetHashCode()
    );
    private static readonly HashTracker hashLocationData = new(
        nameof(Game1.locations),
        static () => Game1.locationData.GetHashCode()
    );
    private static int lastUpdatedTick = -1;

    internal static bool CheckLastUpdatedTick(ref int lastUpdate)
    {
        GetLocationInfo();
        if (lastUpdate != lastUpdatedTick)
        {
            lastUpdate = lastUpdatedTick;
            return true;
        }
        return false;
    }

    private static Dictionary<string, LocationInfo>? cache = null;
    public static IReadOnlyDictionary<string, LocationInfo> Cache => GetLocationInfo();

    private static IReadOnlyDictionary<string, LocationInfo> GetLocationInfo()
    {
        Dictionary<string, LocationInfo> cacheRet = [];
        if (!Context.IsWorldReady)
            return cacheRet;

        Stopwatch? stopwatch = null;

        if (hashLocations.CheckChanged() || cache == null)
        {
            hashLocationData.CheckChanged();
            stopwatch = Stopwatch.StartNew();
            cacheRet = cache = RefreshCache();
            lastUpdatedTick = Game1.ticks;
        }
        else
        {
            cacheRet = cache;
            if (hashLocationData.CheckChanged())
            {
                foreach (LocationInfo locationInfo in cacheRet.Values)
                {
                    locationInfo.ReloadLocationData();
                }
                lastUpdatedTick = Game1.ticks;
            }
        }

        if (stopwatch != null)
            ModEntry.Log($"LocationInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Info);
        return cacheRet;
    }

    private static Dictionary<string, LocationInfo> RefreshCache()
    {
        Dictionary<string, LocationInfo> newCache = [];
        foreach (GameLocation location in Game1.locations)
        {
            if (location.Name != location.NameOrUniqueName)
                continue;
            // populate the game's cache too
            Game1._locationLookup.TryAdd(location.Name, location);
            LocationInfo locInfo = new(location.Name, location);
            newCache[location.Name] = locInfo;
            locInfo.ReloadLocationData();
        }
        return newCache;
    }
}
