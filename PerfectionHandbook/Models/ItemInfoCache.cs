using System.Diagnostics;
using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace PerfectionHandbook.Models;

public sealed record FishSpawnReq(
    bool? Rain,
    int MinFishing,
    IReadOnlyList<(int, int)> TimeRanges,
    IReadOnlyList<string>? CrabPotGroups
);

public sealed record NeededForInfo(int Count, CraftingRecipe Recipe, ItemInfo ResultItem);

public sealed record NeededForInfoGroup(
    ItemInfo ReprInfo,
    string RawId,
    Func<NeededForInfoGroup, PlayerOwned, int> GetOwnedFunc
)
{
    public List<NeededForInfo> Recipes = [];

    public string? CraftingDesc => Recipes.FirstOrDefault()?.Recipe.getNameFromIndex(RawId);

    public List<NeededForInfo> GetNotYetCrafted(Farmer who) =>
        Recipes.Where(recipe => recipe.Recipe.GetRecipeCraftedCount(recipe.ResultItem, who) <= 0).ToList();

    public int GetOwned(PlayerOwned owned) => GetOwnedFunc(this, owned);
}

public sealed record ItemInfo(ParsedItemData Datum)
{
    public Item ReprItem = ItemRegistry.Create(Datum.QualifiedItemId);
    public bool IsPotentialShipped = ItemInfoCache.IsPotentialBasicShipped(Datum);
    public bool IsMuseumDonation = ItemInfoCache.IsMuseumDonation(Datum);
    public bool IsCatchableFish = ItemInfoCache.IsCatchableFish(Datum);

    public bool CountForPolyculture = false;
    public bool CountForMonoculture = false;

    public List<CraftingRecipe> FromRecipe = [];
    public Dictionary<string, CropData> FromCrop = [];
    public List<(LocationInfo, SpawnFishData)> FromFishing = [];
    public FishSpawnReq? FishReq = null;

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return Datum.DisplayName.ContainsIgnoreCase(txt);
    }
}

public static class ItemInfoCache
{
    private static readonly HashTracker hashObject = new(
        nameof(Game1.objectData),
        static () => Game1.objectData.GetHashCode()
    );
    private static readonly HashTracker hashCooking = new(
        nameof(CraftingRecipe.cookingRecipes),
        static () => CraftingRecipe.cookingRecipes.GetHashCode()
    );
    private static readonly HashTracker hashCrafting = new(
        nameof(CraftingRecipe.craftingRecipes),
        static () => CraftingRecipe.craftingRecipes.GetHashCode()
    );
    private static readonly HashTracker hashCrop = new(
        nameof(Game1.cropData),
        static () => Game1.cropData.GetHashCode()
    );
    private static readonly InvalidateTracker invalFish = InvalidateTracker.GetInvalidateTracker("Data/Fish");
    private static int lastLocationUpdatedTick = -2;

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

    private static Dictionary<string, ItemInfo>? cache = null;
    public static IReadOnlyDictionary<string, ItemInfo> Cache => GetItemInfo();
    private static readonly Dictionary<string, NeededForInfoGroup> neededForRecipe = [];
    public static Dictionary<string, NeededForInfoGroup> NeededForRecipe
    {
        get
        {
            GetItemInfo();
            return neededForRecipe;
        }
    }

    internal static IReadOnlyDictionary<string, ItemInfo> GetItemInfo()
    {
        Stopwatch? stopwatch = null;

        Dictionary<string, ItemInfo> cacheRet;
        bool useCached = false;
        if (hashObject.CheckChanged() || cache == null)
        {
            stopwatch = Stopwatch.StartNew();
            cacheRet = cache = RefreshCache();
        }
        else
        {
            cacheRet = cache;
            useCached = true;
        }

        UpdateFromRecipes(cacheRet, useCached);
        UpdateFromCrop(cacheRet, useCached);
        UpdateFromLocation(cacheRet, useCached);
        UpdateFishReq(cacheRet, useCached);

        if (stopwatch != null)
            ModEntry.Log($"ItemInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Info);
        return cacheRet;
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

    public static int GetRecipeCraftedCount(this CraftingRecipe recipe, ItemInfo itemInfo, Farmer who)
    {
        if (recipe.isCookingRecipe)
        {
            if (!who.cookingRecipes.ContainsKey(recipe.name))
                return -1;
            return who.recipesCooked.GetValueOrDefault(itemInfo.Datum.ItemId, 0);
        }
        else
        {
            if (who.craftingRecipes.TryGetValue(recipe.name, out int crafted))
            {
                return crafted;
            }
            else
            {
                return -1;
            }
        }
    }

    private static void UpdateFromRecipes(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        bool cookingChanged = hashCooking.CheckChanged();
        bool craftingChanged = hashCrafting.CheckChanged();
        if (!cookingChanged && !craftingChanged && useCached)
            return;

        ModEntry.LogDebug($"UpdateFromRecipes({useCached})");
        // when using prior cache, clear previous recipe data
        if (useCached)
        {
            foreach (ItemInfo itemInfo in cacheRet.Values)
            {
                itemInfo.FromRecipe.Clear();
            }
        }
        neededForRecipe.Clear();
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

                foreach ((string ingrediantId, int count) in recipe.recipeList)
                {
                    string? key = null;
                    ItemInfo? ingredientInfo = null;
                    Func<NeededForInfoGroup, PlayerOwned, int>? getOwned = null;
                    if (int.TryParse(ingrediantId, out int ingredientNum))
                    {
                        if (ingredientNum == -777)
                        {
                            if (!newCache.TryGetValue("(O)495", out ingredientInfo))
                                continue;
                            getOwned = static (info, owned) =>
                            {
                                int ownedCount = 0;
                                if (owned.OwnedGroups.TryGetValue("(O)495", out OwnedItemGroup? group))
                                    ownedCount += group.CountRepr.ReprStack;
                                if (owned.OwnedGroups.TryGetValue("(O)496", out group))
                                    ownedCount += group.CountRepr.ReprStack;
                                if (owned.OwnedGroups.TryGetValue("(O)497", out group))
                                    ownedCount += group.CountRepr.ReprStack;
                                if (owned.OwnedGroups.TryGetValue("(O)495", out group))
                                    ownedCount += group.CountRepr.ReprStack;
                                return ownedCount;
                            };
                            key = $"{ModEntry.ModId}/wild_seeds";
                        }
                        else if (ingredientNum < 0)
                        {
                            ingredientInfo = newCache.Values.FirstOrDefault(itemInfo =>
                                itemInfo.Datum.Category == ingredientNum
                            );
                            if (ingredientInfo == null)
                                continue;
                            getOwned = static (info, owned) =>
                            {
                                int ownedCount = 0;
                                foreach ((string itemId, OwnedItemGroup group) in owned.OwnedGroups)
                                {
                                    if (group.CountRepr.Category.ToString() == info.RawId)
                                        ownedCount += group.CountRepr.ReprStack;
                                }
                                return ownedCount;
                            };
                            key = $"{ModEntry.ModId}/category_{ingredientNum}";
                        }
                    }
                    if (key == null || ingredientInfo == null || getOwned == null)
                    {
                        string qId = ItemRegistry.QualifyItemId(ingrediantId);
                        if (qId == null)
                        {
                            // spacecore recipe overrides?
                            key = $"{ModEntry.ModId}/contexttag_{ingrediantId}";
                            ingredientInfo = newCache.Values.FirstOrDefault(itemInfo =>
                                itemInfo.ReprItem.HasContextTag(ingrediantId)
                            );
                            if (ingredientInfo == null)
                            {
                                ModEntry.LogOnce(
                                    $"Invalid ingredient '{ingrediantId}' for recipe '{recipe.name}'.",
                                    LogLevel.Warn
                                );
                                continue;
                            }
                            getOwned = static (info, owned) =>
                            {
                                int ownedCount = 0;
                                foreach ((string itemId, OwnedItemGroup group) in owned.OwnedGroups)
                                {
                                    if (group.CountRepr.HasContextTag(info.RawId))
                                        ownedCount += group.CountRepr.ReprStack;
                                }
                                return ownedCount;
                            };
                        }
                        else
                        {
                            if (!newCache.TryGetValue(qId, out ingredientInfo))
                                continue;
                            key = qId;
                            getOwned = static (info, owned) =>
                            {
                                if (
                                    owned.OwnedGroups.TryGetValue(
                                        info.ReprInfo.Datum.QualifiedItemId,
                                        out OwnedItemGroup? group
                                    )
                                )
                                    return group.CountRepr.ReprStack;
                                return 0;
                            };
                        }
                    }
                    if (!neededForRecipe.TryGetValue(key, out NeededForInfoGroup? neededForGroup))
                    {
                        neededForGroup = new(ingredientInfo, ingrediantId, getOwned);
                        neededForRecipe[key] = neededForGroup;
                    }
                    neededForGroup.Recipes.Add(new(count, recipe, itemInfo));
                }
            }
        }
    }

    private static void UpdateFromCrop(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        if (!hashCrop.CheckChanged() && useCached)
            return;
        // when using prior cache, clear previous crop data
        if (useCached)
            foreach (ItemInfo itemInfo in cacheRet.Values)
            {
                itemInfo.CountForMonoculture = false;
                itemInfo.CountForPolyculture = false;
                itemInfo.FromCrop.Clear();
            }
        IAssetName cropSheetName = ModEntry.help.GameContent.ParseAssetName(Game1.cropSpriteSheet.Name);
        foreach ((string seedId, CropData cropData) in Game1.cropData)
        {
            // hardcoding: skip vanilla wild seeds
            if (cropSheetName.IsEquivalentTo(cropData.Texture) && cropData.SpriteIndex == 23)
                continue;
            string? qId = ItemRegistry.QualifyItemId(cropData.HarvestItemId);
            if (qId == null)
            {
                ModEntry.LogOnce($"Failed to qualify HarvestItemId '{qId}' from crop '{seedId}'", LogLevel.Warn);
                continue;
            }
            if (!cacheRet.TryGetValue(qId, out ItemInfo? itemInfo))
            {
                continue;
            }
            itemInfo.CountForPolyculture |= cropData.CountForPolyculture;
            itemInfo.CountForMonoculture |= cropData.CountForMonoculture;
            itemInfo.FromCrop[seedId] = cropData;
        }
    }

    internal static void ClearLocationCache()
    {
        if (cache == null)
            return;
        foreach (ItemInfo itemInfo in cache.Values)
        {
            itemInfo.FromFishing.Clear();
        }
    }

    private static void UpdateFromLocation(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        if (!Context.IsWorldReady)
            return;
        if (!LocationInfoCache.CheckLastUpdatedTick(ref lastLocationUpdatedTick) && useCached)
            return;
        if (useCached)
            foreach (ItemInfo itemInfo in cacheRet.Values)
            {
                itemInfo.FromFishing.Clear();
            }
        ModEntry.LogDebug($"UpdateFromLocation({useCached})");

        foreach (LocationInfo locationInfo in LocationInfoCache.Cache.Values)
        {
            if (!(locationInfo.Fishes?.Any() ?? false))
                continue;
            foreach ((string qId, SpawnFishData spawnFishData) in locationInfo.Fishes)
            {
                if (cacheRet.TryGetValue(qId, out ItemInfo? itemInfo))
                {
                    itemInfo.FromFishing.Add((locationInfo, spawnFishData));
                }
            }
        }
    }

    private static void UpdateFishReq(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        if (!invalFish.CheckChanged() && useCached)
            return;
        Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
        foreach (ItemInfo itemInfo in cacheRet.Values)
        {
            itemInfo.FishReq = null;
            if (!itemInfo.IsCatchableFish)
                continue;
            if (!allFishData.TryGetValue(itemInfo.Datum.ItemId, out string? fishReqStr))
                continue;

            string[] fishReqs = fishReqStr.Split('/');

            List<string>? crabPotsList = null;
            List<(int, int)> timeRanges = [];
            bool? rain = null;

            if (
                ArgUtility.Get(fishReqs, 1) == "trap"
                && ArgUtility.TryGet(fishReqs, 4, out string crabPots, out _, name: "string crabPots")
            )
            {
                crabPotsList = ArgUtility.SplitBySpace(crabPots).ToList();
            }

            if (
                !ArgUtility.TryGet(
                    fishReqs,
                    5,
                    out string rawTimeSpansStr,
                    out _,
                    allowBlank: true,
                    "string rawTimeSpans"
                )
            )
            {
                string[] rawTimeSpans = ArgUtility.SplitBySpace(rawTimeSpansStr);
                for (int i = 0; i < rawTimeSpans.Length; i += 2)
                {
                    if (
                        !ArgUtility.TryGetInt(rawTimeSpans, i, out var startTime, out _, "int startTime")
                        || !ArgUtility.TryGetInt(rawTimeSpans, i + 1, out var endTime, out _, "int endTime")
                    )
                        break;
                    timeRanges.Add((startTime, endTime));
                }
            }

            if (ArgUtility.TryGet(fishReqs, 7, out string weather, out _, allowBlank: true, "string weather"))
            {
                if (weather == "rainy")
                    rain = true;
                else if (weather == "sunny")
                    rain = false;
            }

            ArgUtility.TryGetInt(fishReqs, 12, out int minFishing, out _, "int minFishingLevel");

            itemInfo.FishReq = new(rain, minFishing, timeRanges, crabPotsList);
        }
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
