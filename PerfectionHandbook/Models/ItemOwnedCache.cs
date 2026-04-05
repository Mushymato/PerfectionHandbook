using System.Diagnostics;
using StardewValley;
using StardewValley.Objects;

namespace PerfectionHandbook.Models;

public sealed record OwnedItem(Item ThisItem, Chest? Container = null);

public sealed record OwnedItemGroup(IReadOnlyList<OwnedItem> Things)
{
    public Item ReprItem = GetReprItem(Things);

    private static Item GetReprItem(IReadOnlyList<OwnedItem> OwnedList)
    {
        Item reprItem = OwnedList[0].ThisItem.getOne();
        reprItem.Stack = OwnedList.Sum(owned => owned.ThisItem.Stack);
        return reprItem;
    }
}

public sealed record PlayerOwned(
    Farmer Who,
    IReadOnlyDictionary<string, OwnedItemGroup> OwnedGroups,
    IList<Item> OwnedRepr
);

public static class ItemOwnedCache
{
    public static PlayerOwned GetPlayerOwned(Farmer who)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Dictionary<string, List<OwnedItem>> ownedItems = [];
        Utility.ForEachItemContext(
            (in ctx) =>
            {
                if (ctx.Item == null)
                    return true;
                foreach (object path in ctx.GetPath())
                {
                    OwnedItem? newOwned = null;
                    if (path is Chest chest && chest.playerChest.Value == true)
                    {
                        newOwned = new(ctx.Item, chest);
                    }
                    else if (path == who)
                    {
                        newOwned = new(ctx.Item, null);
                    }
                    if (newOwned != null)
                    {
                        ownedItems.TryAdd(ctx.Item.QualifiedItemId, []);
                        ownedItems[ctx.Item.QualifiedItemId].Add(newOwned);
                    }
                }
                return true;
            }
        );
        var ownedItemGroups = ownedItems.ToDictionary(kv => kv.Key, kv => new OwnedItemGroup(kv.Value));
        PlayerOwned result = new(who, ownedItemGroups, ownedItemGroups.Values.Select(value => value.ReprItem).ToList());

        ModEntry.Log($"OwnedItems: gathered in {stopwatch.Elapsed}");
        return result;
    }
}
