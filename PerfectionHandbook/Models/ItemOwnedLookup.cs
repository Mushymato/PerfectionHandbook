using System.Diagnostics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Objects;

namespace PerfectionHandbook.Models;

public sealed record OwnedItem(Item ThisItem, Chest? Container = null);

public sealed record OwnedItemGroup(IReadOnlyList<OwnedItem> Things)
{
    public readonly ReprObject CountRepr = GetCountRepr(Things, true);
    public readonly ReprObject CountWithoutInventoryRepr = GetCountRepr(Things, false);

    private static ReprObject GetCountRepr(IReadOnlyList<OwnedItem> OwnedList, bool includePlayerInventory)
    {
        ReprObject reprItem = new(OwnedList[0].ThisItem.getOne());
        reprItem.SetReprStack(
            OwnedList.Sum(owned => includePlayerInventory || owned.Container != null ? owned.ThisItem.Stack : 0)
        );
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
        Dictionary<string, OwnedItemGroup> ownedItemGroups = [];
        foreach ((string key, List<OwnedItem> things) in ownedItems)
        {
            try
            {
                ownedItemGroups[key] = new(things);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Failed to make OwnedItemGroup for '{key}'\n{ex}", LogLevel.Error);
                continue;
            }
        }
        PlayerOwned result = new(
            ownedItemGroups,
            ownedItemGroups.Values.Select(value => (Item)value.CountWithoutInventoryRepr).ToList()
        );

        ModEntry.Log($"OwnedItems({Game1.ticks}): gathered in {stopwatch.Elapsed}", LogLevel.Debug);
        return result;
    }
}
