using System.Diagnostics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;

namespace PerfectionHandbook.Models;

public record NPCInfo(string Name, CharacterData Data, NPC Chara)
{
    public readonly bool CountForPerfection =
        Data.PerfectionScore && !GameStateQuery.IsImmutablyFalse(Data.CanSocialize);
    public readonly int MaxPoints = (Data.CanBeRomanced ? 8 : 10) * 250;
}

public static class NPCInfoCache
{
    private static readonly HashTracker hashCharacters = new(
        nameof(Game1.characterData),
        static () => Game1.characterData.GetHashCode()
    );

    private static Dictionary<string, NPCInfo>? cache = null;
    public static IReadOnlyDictionary<string, NPCInfo> Cache => GetNPCInfo();

    internal static IReadOnlyDictionary<string, NPCInfo> GetNPCInfo()
    {
        Stopwatch? stopwatch = null;

        Dictionary<string, NPCInfo> cacheRet;

        // bool useCached = false;
        if (hashCharacters.CheckChanged() || cache == null)
        {
            stopwatch = Stopwatch.StartNew();
            cacheRet = cache = RefreshCache();
        }
        else
        {
            cacheRet = cache;
            // useCached = true;
        }

        if (stopwatch != null)
            ModEntry.Log($"NPCInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Info);
        return cacheRet;
    }

    private static Dictionary<string, NPCInfo> RefreshCache()
    {
        Dictionary<string, NPCInfo> cacheRet = [];
        Utility.ForEachVillager(chara =>
        {
            if (!Game1.characterData.TryGetValue(chara.Name, out CharacterData? data))
                return true;
            cacheRet[chara.Name] = new(chara.Name, data, chara);
            return true;
        });
        return cacheRet;
    }
}
