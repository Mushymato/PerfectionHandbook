using System.Diagnostics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
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

public static class ItemOwnedLookup
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
                if (ctx.Item.QualifiedItemId == "(O)176")
                {
                    ModEntry.Log($"{string.Join('>', ctx.GetDisplayPath())}");
                }

                foreach (object path in ctx.GetPath().Reverse())
                {
                    if (path is Chest chest && chest.playerChest.Value == true)
                    {
                        AddToOwnedItems(ctx, ownedItems, new(ctx.Item, chest));
                    }
                    else if (path is Farmer)
                    {
                        AddToOwnedItems(ctx, ownedItems, new(ctx.Item, null));
                    }
                    else if (path is GameLocation loc)
                    {
                        // special case: the fridge (Chest) does not get put in ForEachItemContext, weird
                        if (loc.GetFridge() is Chest fridge && fridge.Items.Contains(ctx.Item))
                        {
                            AddToOwnedItems(ctx, ownedItems, new(ctx.Item, fridge));
                        }
                        break;
                    }
                }
                return true;

                static void AddToOwnedItems(
                    ForEachItemContext ctx,
                    Dictionary<string, List<OwnedItem>> ownedItems,
                    OwnedItem newOwned
                )
                {
                    ownedItems.TryAdd(ctx.Item.QualifiedItemId, []);
                    ownedItems[ctx.Item.QualifiedItemId].Add(newOwned);
                }
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
