using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.Models;

public sealed record ItemInfo(ParsedItemData Datum)
{
    public Item ReprItem = ItemRegistry.Create(Datum.QualifiedItemId);
    public bool IsPotentialShipped = ItemCache.IsPotentialShipped(Datum);
}

public static class ItemCache
{
    private static int lastObjectDataHash = -1;
    private static List<ItemInfo>? cache = null;
    public static List<ItemInfo> Cache
    {
        get
        {
            int newObjectDataHash = Game1.objectData.GetHashCode();
            if (cache != null && lastObjectDataHash == newObjectDataHash)
                return cache;
            ModEntry.Log($"ItemCache refresh: {lastObjectDataHash} -> {newObjectDataHash}");
            lastObjectDataHash = newObjectDataHash;
            cache = [];
            foreach (ParsedItemData datum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                cache.Add(new(datum));
            }
            return cache;
        }
    }

    public static bool IsPotentialShipped(ParsedItemData datum)
    {
        int category = datum.Category;
        return category != SObject.CookingCategory
            && category != SObject.GemCategory
            && SObject.isPotentialBasicShipped(datum.ItemId, category, datum.ObjectType);
    }
}
