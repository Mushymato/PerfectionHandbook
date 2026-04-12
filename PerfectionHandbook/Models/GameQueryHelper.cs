using Force.DeepCloner;
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
        List<string> parts = ArgUtility
            .SplitQuoteAware(condition, ' ', StringSplitOptions.None, keepQuotesAndEscapes: true)
            .ToList();
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[0].EqualsIgnoreCase("here"))
                parts[0] = "Target";
        }
        return GameStateQuery.CheckConditions(
            ArgUtility.UnsplitQuoteAware(parts.ToArray(), ' '),
            location: location,
            ignoreQueryKeys: GSQRandomKeys
        );
    }
}
