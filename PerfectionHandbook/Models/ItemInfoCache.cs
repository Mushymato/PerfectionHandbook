using System.Diagnostics;
using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace PerfectionHandbook.Models;

public sealed record ItemInfo(ParsedItemData Datum)
{
    public Item ReprItem = ItemRegistry.Create(Datum.QualifiedItemId);
    public bool IsPotentialShipped = ItemInfoCache.IsPotentialBasicShipped(Datum);
    public bool IsMuseumDonation =
        Datum.GetItemTypeId() == ItemRegistry.type_object && LibraryMuseum.IsItemSuitableForDonation(Datum.ItemId);
    public bool IsCatchableFish =
        Datum.ObjectType == "Fish" && !(Datum.RawData is ObjectData { ExcludeFromFishingCollection: not false });
    public List<CraftingRecipe> FromRecipe = [];
}

public static class ItemInfoCache
{
    private static int lastObjectDataHash = -1;
    private static int lastCraftingHash = -1;
    private static int lastCookingHash = -1;

    private static Func<string, bool, CraftingRecipe> MakeCraftingRecipe = Vanilla_MakeCraftingRecipe;

    private static CraftingRecipe Vanilla_MakeCraftingRecipe(string recipeId, bool isCooking) =>
        new(recipeId, isCooking);

    public static void Setup()
    {
        if (
            ModEntry.help.ModRegistry.Get("spacechase0.SpaceCore") is IModInfo modInfo
            && modInfo.GetType()?.GetProperty("Mod")?.GetValue(modInfo) is IMod mod
        )
        {
            Assembly assembly = mod.GetType().Assembly;
            if (
                assembly
                    .GetType("SpaceCore.Patches.CraftingRecipePatcher")
                    ?.GetMethod("RedirectedCreateRecipe", BindingFlags.Static | BindingFlags.NonPublic)
                is MethodInfo makeCraft
            )
            {
                ModEntry.Log($"Create recipes with: {makeCraft}");
                cache = null;
                MakeCraftingRecipe = makeCraft.CreateDelegate<Func<string, bool, CraftingRecipe>>();
            }
        }
        else
        {
            ModEntry.Log($"Create recipes with: vanilla");
        }
    }

    private static bool HashHasChanged()
    {
        bool anyHashChanged = false;
        // objects
        int newObjectDataHash = Game1.objectData.GetHashCode();
        if (lastObjectDataHash != newObjectDataHash)
        {
            ModEntry.Log($"Game1.objectData.GetHashCode(): {lastObjectDataHash} -> {newObjectDataHash}");
            anyHashChanged = true;
        }
        lastObjectDataHash = newObjectDataHash;
        // cooking
        int newCookingHash = CraftingRecipe.cookingRecipes.GetHashCode();
        if (lastCookingHash != newCookingHash)
        {
            ModEntry.Log($"CraftingRecipe.cookingRecipes.GetHashCode(): {lastCookingHash} -> {newCookingHash}");
            anyHashChanged = true;
        }
        lastCookingHash = newCookingHash;
        // crafting
        int newCraftingHash = CraftingRecipe.craftingRecipes.GetHashCode();
        if (lastCraftingHash != newCraftingHash)
        {
            ModEntry.Log($"CraftingRecipe.newCraftingHash.GetHashCode(): {lastCraftingHash} -> {newCraftingHash}");
            anyHashChanged = true;
        }
        lastCraftingHash = newCraftingHash;
        return anyHashChanged;
    }

    private static IReadOnlyDictionary<string, ItemInfo>? cache = null;
    public static IReadOnlyDictionary<string, ItemInfo> Cache
    {
        get
        {
            if (!HashHasChanged() && cache != null)
                return cache;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Dictionary<string, ItemInfo> newCache = [];
            // objects
            foreach (ParsedItemData datum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                newCache[datum.QualifiedItemId] = new(datum);
            }
            // cooking
            PopulateRecipes(newCache, true);
            // crafting
            PopulateRecipes(newCache, false);
            cache = newCache;
            ModEntry.Log($"ItemInfoCache: refreshed in {stopwatch.Elapsed}");
            return cache;

            static void PopulateRecipes(Dictionary<string, ItemInfo> newCache, bool isCooking)
            {
                var recipeIds = (isCooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes).Keys;
                foreach (string recipeId in recipeIds)
                {
                    CraftingRecipe recipe = MakeCraftingRecipe(recipeId, isCooking);
                    Item reprItem = recipe.createItem(); // must do this to account for spacecore
                    ParsedItemData datum = ItemRegistry.GetDataOrErrorItem(reprItem.QualifiedItemId);
                    if (datum.IsErrorItem)
                        continue;
                    if (!newCache.TryGetValue(datum.QualifiedItemId, out ItemInfo? itemInfo))
                    {
                        itemInfo = new(datum);
                        newCache[datum.QualifiedItemId] = itemInfo;
                    }
                    itemInfo.FromRecipe.Add(recipe);
                }
            }
        }
    }

    public static bool IsPotentialBasicShipped(ParsedItemData datum)
    {
        if (datum.GetItemTypeId() != ItemRegistry.type_object)
            return false;
        int category = datum.Category;
        return category != SObject.CookingCategory
            && category != SObject.GemCategory
            && SObject.isPotentialBasicShipped(datum.ItemId, category, datum.ObjectType);
    }
}
