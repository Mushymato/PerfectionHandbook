using Force.DeepCloner;
using PerfectionHandbook;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;

internal static class GameQueryHelper
{
    internal static IReadOnlyList<ParsedItemData> SimplifiedResolveAll(
        GenericSpawnItemData spawn,
        GameLocation? location
    )
    {
        List<ParsedItemData> resolvedItemData = [];
        List<string> needsIQR = [];
        SimpleOrIQR(spawn.ItemId, resolvedItemData, needsIQR);
        if (spawn.RandomItemId?.Count > 0)
        {
            foreach (string randId in spawn.RandomItemId)
            {
                SimpleOrIQR(randId, resolvedItemData, needsIQR);
            }
        }
        if (needsIQR.Count == 0)
        {
            return resolvedItemData;
        }
        GenericSpawnItemData tmpSpawn = spawn.ShallowClone();
        tmpSpawn.RandomItemId = null;
        ItemQueryContext iqContext = new(location, null, Random.Shared, "SimplifiedResolveAll");
        foreach (string needIQR in needsIQR)
        {
            ModEntry.Log($"Try resolve on: {needIQR}");
            tmpSpawn.ItemId = needIQR;
            foreach (ItemQueryResult res in ItemQueryResolver.TryResolve(tmpSpawn, iqContext))
            {
                if (ItemRegistry.GetData(res.Item.QualifiedItemId) is ParsedItemData parsed)
                {
                    resolvedItemData.Add(parsed);
                }
            }
        }
        return resolvedItemData;
    }

    private static void SimpleOrIQR(string? spawnItemId, List<ParsedItemData> result, List<string> needsIQR)
    {
        if (spawnItemId == null)
            return;
        if (ItemRegistry.GetData(spawnItemId) is ParsedItemData parsed)
            result.Add(parsed);
        else
            needsIQR.Add(spawnItemId);
    }

    internal static readonly HashSet<string> GSQRandomKeys =
    [
        "RANDOM", /*"SYNCED_RANDOM"*/
    ];

    internal static bool GSQCheckNoRandom(string condition, GameLocation? location = null)
    {
        return GameStateQuery.CheckConditions(condition, location: location, ignoreQueryKeys: GSQRandomKeys);
    }
}
