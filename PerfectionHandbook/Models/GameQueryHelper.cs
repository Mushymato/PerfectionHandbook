using Force.DeepCloner;
using PerfectionHandbook;
using StardewValley;
using StardewValley.Extensions;
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

    internal static bool ContextLocationCheckNoRandom(string condition, GameLocation location)
    {
        string[] parts = condition.Split(' ');
        List<string> replaced = [];
        foreach (string part in parts)
        {
            if (part.EqualsIgnoreCase("here"))
                replaced.Add("Target");
            else
                replaced.Add(part);
        }
        return GameStateQuery.CheckConditions(
            string.Join(' ', replaced),
            location: location,
            ignoreQueryKeys: GSQRandomKeys
        );
    }
}
