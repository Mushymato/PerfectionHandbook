using StardewValley;
using StardewValley.GameData.Locations;

namespace PerfectionHandbook.Models;

public sealed record FishSpawnInfo(string FishId, SpawnFishData FishSpawn);

public sealed record LocationInfo(List<FishSpawnInfo> Fish) { }

public static class LocationInfoCache
{
    private static readonly HashTracker hashLocation = new(
        nameof(Game1.locationData),
        static () => Game1.locationData.GetHashCode()
    );

    public static void Setup() { }
}
