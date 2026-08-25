using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.Models;

public sealed record EventPreconditionInfo(
    string Precond,
    bool Negated,
    string[] Args,
    EventPreconditionDelegate Handler
)
{
    private readonly ArgGetInfo argGetInfo = DelegateInspector.ExtractTryGetPairs(Handler);
    public string DisplayText => $"{(Negated ? '!' : "")}{Handler.Method.Name}:{argGetInfo.FormArgDesc(Negated, Args)}";

    public bool Evaluate(EventInfo eventInfo)
    {
        if (LocationInfoCache.Cache.TryGetValue(eventInfo.LocationId, out LocationInfo? locationInfo))
        {
            return Handler(locationInfo.Location, eventInfo.EventId, Args) == !Negated;
        }
        return false;
    }
}

public sealed record EventInfo(
    string EventId,
    EventPreconditionInfo[] Preconditions,
    string[] Commands,
    string[] Actors,
    string LocationId,
    string LocationName,
    string EventKey,
    IModNameInfo? ModNameInfo
)
{
    public readonly string HeaderText = $"{EventId} @ {LocationName}";

    public bool HasModName => ModNameInfo != null;
    public string ModName => ModNameInfo?.ModName ?? string.Empty;
    public Color ModNameTint => ModNameInfo?.ModNameColor ?? Game1.textColor;

    public static bool TryParse(
        string locationId,
        string locationName,
        string key,
        string script,
        IAssetName assetName,
        [NotNullWhen(true)] out EventInfo? info
    )
    {
        info = null;

        string[] idPrecond = Event.SplitPreconditions(key);
        string eventId = idPrecond[0];
        if (!TryNormalizePrecond(idPrecond.Skip(1), out EventPreconditionInfo[]? preconds))
        {
            return false;
        }
        string[] commands = Event.ParseCommands(script);
        List<string> actors = [];

        if (
            ArgUtility.TryGet(
                commands,
                2,
                out string setrawCharacterPositionsupChara,
                out _,
                allowBlank: false,
                "string rawCharacterPositions"
            )
        )
        {
            string[] array = ArgUtility.SplitBySpace(setrawCharacterPositionsupChara);
            for (int i = 0; i < array.Length; i += 4)
            {
                if (!ArgUtility.TryGet(array, i, out string actorName, out _, allowBlank: true, "string actorName"))
                {
                    continue;
                }
                actors.Add(actorName);
            }
        }

        info = new(
            eventId,
            preconds,
            commands,
            actors.ToArray(),
            locationId,
            locationName,
            key,
            ModEntry.modNameAPI?.GetModName_FromAssetAndId(assetName, eventId)
        );
        return true;

        static bool TryNormalizePrecond(
            IEnumerable<string> preconds,
            [NotNullWhen(true)] out EventPreconditionInfo[]? normalized
        )
        {
            normalized = null;
            List<EventPreconditionInfo> normalizedList = [];
            foreach (string precond in preconds)
            {
                string[] parts = ArgUtility.SplitBySpaceQuoteAware(precond);
                string realPrecond = parts[0];
                bool negated = false;
                if (realPrecond.StartsWith('!'))
                {
                    realPrecond = realPrecond[1..];
                    negated = true;
                }
                if (!Event.TryGetPreconditionHandler(realPrecond, out EventPreconditionDelegate handler))
                {
                    return false;
                }
                normalizedList.Add(new(realPrecond, negated, parts, handler));
            }
            normalized = normalizedList.ToArray();
            return true;
        }
    }
}

public sealed record LocationInfo(string LocationId, GameLocation Location)
{
    public LocationData? Data { get; private set; }
    public IReadOnlyDictionary<string, SpawnFishData>? Fishes { get; private set; }
    private InvalidateTracker? EventInvalidateTracker;
    public IReadOnlyDictionary<string, EventInfo>? Events { get; private set; }
    public bool HasWater { get; set; } = false;

    public void ReloadLocationData(out bool hasNewEvent)
    {
        hasNewEvent = default;
        Fishes = null;
        Data = Location.GetData();
        if (Data == null)
            return;

        // water tiles
        HasWater = false;
        if (Location.waterTiles != null)
        {
            foreach (WaterTiles.WaterTileData waterTile in Location.waterTiles.waterTiles)
            {
                if (waterTile.isWater)
                {
                    HasWater = true;
                    break;
                }
            }
        }
        else if (
            (Location.IsOutdoors || Location.HasMapPropertyWithValue("indoorWater"))
            && Location.Map?.Layers?.Count > 0
        )
        {
            // need this check because desert >:(
            xTile.Layers.Layer layer = Location.Map.Layers[0];
            for (int i = 0; i < layer.LayerWidth; i++)
            {
                for (int j = 0; j < layer.LayerHeight; j++)
                {
                    if (layer.Tiles[i, j] is not xTile.Tiles.Tile tile)
                        continue;
                    if (tile.Properties.ContainsKey("Water") || tile.TileIndexProperties.ContainsKey("Water"))
                    {
                        HasWater = true;
                        break;
                    }
                }
            }
        }

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

        // events
        if (
            (EventInvalidateTracker == null || EventInvalidateTracker.CheckChanged())
            && Location.TryGetLocationEvents(out string assetName, out Dictionary<string, string> events)
        )
        {
            EventInvalidateTracker ??= InvalidateTracker.GetInvalidateTracker(assetName);
            Dictionary<string, EventInfo>? eventsInfo = [];
            foreach ((string key, string commands) in events)
            {
                if (
                    EventInfo.TryParse(
                        LocationId,
                        Location.DisplayName ?? LocationId,
                        key,
                        commands,
                        EventInvalidateTracker.AssetName,
                        out EventInfo? info
                    )
                )
                {
                    eventsInfo[info.EventId] = info;
                }
            }
            Events = eventsInfo;
            hasNewEvent = true;
        }
    }
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
            NPCInfoCache.RefreshEvents(cacheRet.Values);
        }
        else
        {
            cacheRet = cache;
            if (hashLocationData.CheckChanged())
            {
                List<LocationInfo> newEventLocations = [];
                foreach (LocationInfo locationInfo in cacheRet.Values)
                {
                    locationInfo.ReloadLocationData(out bool hasNewEvent);
                    if (hasNewEvent)
                    {
                        newEventLocations.Add(locationInfo);
                    }
                }
                lastUpdatedTick = Game1.ticks;
                NPCInfoCache.RefreshEvents(newEventLocations);
            }
        }

        if (stopwatch != null)
            ModEntry.LogDebug($"LocationInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Debug);

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
            locInfo.ReloadLocationData(out _);
        }
        return newCache;
    }
}
