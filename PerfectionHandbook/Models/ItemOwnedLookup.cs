using System.Diagnostics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace PerfectionHandbook.Models;

public sealed record OwnedItem(Item ThisItem, Chest? Container = null);

public sealed record OwnedItemGroup(
    IReadOnlyList<OwnedItem> Things,
    ReprObject CountRepr,
    ReprObject CountWithoutInventoryRepr,
    ReprObject CountFromOnlyFridge
)
{
    public static OwnedItemGroup Make(IReadOnlyList<OwnedItem> Things)
    {
        int count = 0;
        int countWithoutInventory = 0;
        int countWithOnlyFridge = 0;
        foreach (OwnedItem owned in Things)
        {
            int stack = owned.ThisItem.Stack;
            count += stack;
            if (owned.Container != null)
            {
                countWithoutInventory += stack;
                if (owned.Container.fridge.Value)
                {
                    countWithOnlyFridge += stack;
                }
            }
        }
        Item firstThing = Things[0].ThisItem;
        return new(
            Things,
            new ReprObject(firstThing.getOne()).SetReprStack(count),
            new ReprObject(firstThing.getOne()).SetReprStack(countWithoutInventory),
            new ReprObject(firstThing.getOne()).SetReprStack(countWithOnlyFridge)
        );
    }
}

public sealed record PlayerOwned(
    IReadOnlyDictionary<string, OwnedItemGroup> OwnedGroups,
    IList<Item> OwnedRepr,
    IList<Item> OwnedReprOnlyFridge
);

public static class ItemOwnedLookup
{
    public static PlayerOwned GetPlayerOwned()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Dictionary<string, List<OwnedItem>> ownedItems = ModEntry.config.StrictItemOwnedCheck
            ? GetPlayerOwnedFarmhouse()
            : GetPlayerOwnedEverywhere();
        stopwatch.Stop();
        Dictionary<string, OwnedItemGroup> ownedItemGroups = [];
        foreach ((string key, List<OwnedItem> things) in ownedItems)
        {
            try
            {
                ownedItemGroups[key] = OwnedItemGroup.Make(things);
            }
            catch (Exception ex)
            {
                ModEntry.Log($"Failed to make OwnedItemGroup for '{key}'", LogLevel.Error);
                ModEntry.Log(ex.ToString(), LogLevel.Error);
                continue;
            }
        }

        ModEntry.Log($"OwnedItems({Game1.ticks}): gathered in {stopwatch.Elapsed}", LogLevel.Debug);

        PlayerOwned result = new(
            ownedItemGroups,
            ownedItemGroups.Values.Select(value => (Item)value.CountWithoutInventoryRepr).ToList(),
            ownedItemGroups.Values.Select(value => (Item)value.CountFromOnlyFridge).ToList()
        );

        return result;
    }

    private static Dictionary<string, List<OwnedItem>> GetPlayerOwnedFarmhouse()
    {
        GameLocation farmhouse = Utility.getHomeOfFarmer(Game1.player);
        Dictionary<string, List<OwnedItem>> ownedItems = [];
        foreach (Item item in Game1.player.Items)
        {
            AddToOwnedItems(ownedItems, item, null);
        }
        foreach (SObject obj in farmhouse.objects.Values)
        {
            if (obj is not Chest chest || !chest.playerChest.Value == true)
                continue;
            foreach (Item item in chest.Items)
            {
                AddToOwnedItems(ownedItems, item, chest);
            }
        }
        if (farmhouse.GetFridge() is Chest fridge)
        {
            foreach (Item item in fridge.Items)
            {
                AddToOwnedItems(ownedItems, item, fridge);
            }
        }
        return ownedItems;
    }

    private static Dictionary<string, List<OwnedItem>> GetPlayerOwnedEverywhere()
    {
        Dictionary<string, List<OwnedItem>> ownedItems = [];
        Utility.ForEachItemContext(
            (in ctx) =>
            {
                if (ctx.Item == null)
                    return true;

                foreach (object path in ctx.GetPath().Reverse())
                {
                    Item item = ctx.Item;
                    if (path is Chest chest && chest.playerChest.Value == true)
                    {
                        AddToOwnedItems(ownedItems, item, chest);
                    }
                    else if (path is Farmer)
                    {
                        AddToOwnedItems(ownedItems, item, null);
                    }
                    else if (path is GameLocation loc)
                    {
                        // special case: the fridge (Chest) does not get put in ForEachItemContext, weird
                        if (loc.GetFridge() is Chest fridge && fridge.Items.Contains(ctx.Item))
                        {
                            AddToOwnedItems(ownedItems, item, fridge);
                        }
                        break;
                    }
                }
                return true;
            }
        );
        return ownedItems;
    }

    private static void AddToOwnedItems(
        Dictionary<string, List<OwnedItem>> ownedItems,
        Item? item,
        Chest? container = null
    )
    {
        if (item == null)
            return;
        ownedItems.TryAdd(item.QualifiedItemId, []);
        ownedItems[item.QualifiedItemId].Add(new(item, container));
    }
}
