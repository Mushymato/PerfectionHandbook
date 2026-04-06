using System.Diagnostics;
using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace PerfectionHandbook.Models;

public sealed record ItemSourceInfo(List<Season>? Seasons, string? LocationName);

public sealed record ItemInfo(ParsedItemData Datum)
{
    public Item ReprItem = ItemRegistry.Create(Datum.QualifiedItemId);
    public bool IsPotentialShipped = ItemInfoCache.IsPotentialBasicShipped(Datum);
    public bool IsMuseumDonation = ItemInfoCache.IsMuseumDonation(Datum);
    public bool IsCatchableFish = ItemInfoCache.IsCatchableFish(Datum);

    public bool CountForPolyculture = false;
    public bool CountForMonoculture = false;

    public List<CraftingRecipe> FromRecipe = [];
    public List<CropData> FromCrop = [];

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return Datum.DisplayName.ContainsIgnoreCase(txt);
    }
}

public static class ItemInfoCache
{
    private static int lastObjectDataHash = -1;
    private static int lastCraftingHash = -1;
    private static int lastCookingHash = -1;
    private static int lastCropDataHash = -1;

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

    private static void CheckHashChanged(string logName, int newHash, ref int hashField, ref bool anyHashChanged)
    {
        if (newHash != hashField)
        {
            ModEntry.Log($"{logName}: {hashField} -> {newHash}");
            anyHashChanged = true;
            hashField = newHash;
        }
    }

    private static bool ItemHashChanged()
    {
        bool anyHashChanged = false;
        // objects
        CheckHashChanged(
            nameof(Game1.objectData),
            Game1.objectData.GetHashCode(),
            ref lastObjectDataHash,
            ref anyHashChanged
        );
        return anyHashChanged;
    }

    private static bool RecipeHashChanged()
    {
        bool anyHashChanged = false;
        // cooking
        CheckHashChanged(
            nameof(CraftingRecipe.cookingRecipes),
            CraftingRecipe.cookingRecipes.GetHashCode(),
            ref lastCookingHash,
            ref anyHashChanged
        );
        // crafting
        CheckHashChanged(
            nameof(CraftingRecipe.craftingRecipes),
            CraftingRecipe.craftingRecipes.GetHashCode(),
            ref lastCraftingHash,
            ref anyHashChanged
        );
        return anyHashChanged;
    }

    private static bool CropHashChanged()
    {
        bool anyHashChanged = false;
        // crops
        CheckHashChanged(
            nameof(Game1.cropData),
            Game1.cropData.GetHashCode(),
            ref lastCropDataHash,
            ref anyHashChanged
        );
        return anyHashChanged;
    }

    private static Dictionary<string, ItemInfo>? cache = null;
    public static IReadOnlyDictionary<string, ItemInfo> Cache
    {
        get
        {
            // when menu is active and cache exists, don't do any refresh
            if (Game1.activeClickableMenu != null && cache != null)
                return cache;

            Stopwatch? stopwatch = null;

            Dictionary<string, ItemInfo> cacheRet;
            bool useCached = false;
            if (ItemHashChanged() || cache == null)
            {
                stopwatch = Stopwatch.StartNew();
                cacheRet = cache = RefreshCache();
            }
            else
            {
                cacheRet = cache;
                useCached = true;
            }

            if (RecipeHashChanged())
            {
                UpdateFromRecipes(cacheRet, useCached);
            }

            if (CropHashChanged())
            {
                UpdateFromCrop(cacheRet, useCached);
            }

            if (stopwatch != null)
                ModEntry.Log($"ItemInfoCache: refreshed in {stopwatch.Elapsed}");
            return cacheRet;
        }
    }

    private static void UpdateFromRecipes(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        // when using prior cache, clear previous recipe data
        if (useCached)
            foreach (ItemInfo itemInfo in cacheRet.Values)
                itemInfo.FromRecipe.Clear();
        // cooking
        PopulateRecipes(cacheRet, true);
        // crafting
        PopulateRecipes(cacheRet, false);
        static void PopulateRecipes(Dictionary<string, ItemInfo> newCache, bool isCooking)
        {
            var recipeIds = (isCooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes).Keys;
            foreach (string recipeId in recipeIds)
            {
                CraftingRecipe recipe = MakeCraftingRecipe(recipeId, isCooking);
                Item reprItem = recipe.createItem(); // must do this to account for spacecore
                ParsedItemData datum = ItemRegistry.GetDataOrErrorItem(reprItem.QualifiedItemId);
                if (!newCache.TryGetValue(datum.QualifiedItemId, out ItemInfo? itemInfo))
                {
                    itemInfo = new(datum);
                    newCache[datum.QualifiedItemId] = itemInfo;
                }
                itemInfo.FromRecipe.Add(recipe);
            }
        }
    }

    private static void UpdateFromCrop(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        if (useCached)
            foreach (ItemInfo itemInfo in cacheRet.Values)
            {
                itemInfo.CountForMonoculture = false;
                itemInfo.CountForPolyculture = false;
                itemInfo.FromCrop.Clear();
            }
        foreach (CropData cropData in Game1.cropData.Values)
        {
            if (!cacheRet.TryGetValue(ItemRegistry.QualifyItemId(cropData.HarvestItemId), out ItemInfo? itemInfo))
            {
                continue;
            }
            itemInfo.CountForPolyculture |= cropData.CountForPolyculture;
            itemInfo.CountForMonoculture |= cropData.CountForMonoculture;
            itemInfo.FromCrop.Add(cropData);
        }
    }

    private static Dictionary<string, ItemInfo> RefreshCache()
    {
        Dictionary<string, ItemInfo> newCache = [];
        // objects
        foreach (ParsedItemData datum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
        {
            newCache[datum.QualifiedItemId] = new(datum);
        }
        cache = newCache;
        return cache;
    }

    public static bool IsPotentialBasicShipped(ParsedItemData datum)
    {
        if (datum.IsErrorItem)
            return false;
        if (datum.GetItemTypeId() != ItemRegistry.type_object)
            return false;
        int category = datum.Category;
        return category != SObject.CookingCategory
            && category != SObject.GemCategory
            && SObject.isPotentialBasicShipped(datum.ItemId, category, datum.ObjectType);
    }

    internal static bool IsMuseumDonation(ParsedItemData datum)
    {
        if (datum.IsErrorItem)
            return false;
        if (datum.GetItemTypeId() != ItemRegistry.type_object)
            return false;
        return LibraryMuseum.IsItemSuitableForDonation(datum.ItemId, checkDonatedItems: false);
    }

    internal static bool IsCatchableFish(ParsedItemData datum)
    {
        if (datum.IsErrorItem)
            return false;
        return datum.ObjectType == "Fish" && !(datum.RawData is ObjectData { ExcludeFromFishingCollection: not false });
    }
}
