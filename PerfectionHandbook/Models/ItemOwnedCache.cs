using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace PerfectionHandbook.Models;

public sealed record OwnedItem(Item ThisItem, Chest? Container = null);

public sealed record OwnedItemGroup(IReadOnlyList<OwnedItem> Things)
{
    public ReprObject CountRepr = GetCountRepr(Things);

    private static ReprObject GetCountRepr(IReadOnlyList<OwnedItem> OwnedList)
    {
        ReprObject reprItem = new(OwnedList[0].ThisItem.getOne());
        reprItem.SetReprStack(OwnedList.Sum(owned => owned.ThisItem.Stack));
        return reprItem;
    }
}

public sealed record PlayerOwned(IReadOnlyDictionary<string, OwnedItemGroup> OwnedGroups, IList<Item> OwnedRepr);

public static class ItemOwnedCache
{
    public static PlayerOwned GetPlayerOwned()
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
                    else if (path is Farmer)
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
        PlayerOwned result = new(
            ownedItemGroups,
            ownedItemGroups.Values.Select(value => (Item)value.CountRepr).ToList()
        );

        ModEntry.Log($"OwnedItems({Game1.ticks}): gathered in {stopwatch.Elapsed}", LogLevel.Info);
        return result;
    }
}
