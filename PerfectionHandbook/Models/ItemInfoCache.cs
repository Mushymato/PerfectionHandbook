using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.Integration;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

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
    string CraftingDesc,
    Func<NeededForInfoGroup, PlayerOwned, int> GetOwnedFunc
)
{
    public readonly List<NeededForInfo> NeededFor = [];
    public SDUISprite? ReprIcon = null;

    public List<NeededForInfo> GetNotYetCrafted(Farmer who) =>
        NeededFor.Where(recipe => recipe.Recipe.GetRecipeCraftedCount(recipe.ResultItem, who) <= 0).ToList();

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

    #region spacecore
    private static CraftingRecipe Vanilla_MakeCraftingRecipe(string recipeId, bool isCooking) =>
        new(recipeId, isCooking);

    private static bool isSpacecore = false;
    private static Func<string, bool, CraftingRecipe> MakeCraftingRecipe = Vanilla_MakeCraftingRecipe;
    #endregion

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
                isSpacecore = true;
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
            ModEntry.Log($"ItemInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Debug);
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
            dynamic? spacecoreVAE = null;
            if (isSpacecore)
            {
                string assetName = isCooking
                    ? "spacechase0.SpaceCore/CookingRecipeOverrides"
                    : "spacechase0.SpaceCore/CraftingRecipeOverrides";
                if (Game1.content.DoesAssetExist<dynamic>(assetName))
                {
                    spacecoreVAE = Game1.content.Load<dynamic>(assetName);
                }
            }
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
                    Func<NeededForInfoGroup?>? makeNeedForInfoGroup = null;
                    if (int.TryParse(ingrediantId, out int ingredientNum))
                    {
                        if (ingredientNum == -777)
                        {
                            if (
                                !TryMakeNeededFor_WildSeeds(
                                    newCache,
                                    recipe,
                                    ingrediantId,
                                    ref key,
                                    ref makeNeedForInfoGroup
                                )
                            )
                            {
                                continue;
                            }
                        }
                        else if (ingredientNum < 0)
                        {
                            TryMakeNeededFor_Category(
                                newCache,
                                recipe,
                                ingrediantId,
                                ingredientNum,
                                ref key,
                                ref makeNeedForInfoGroup
                            );
                        }
                    }
                    if (key == null)
                    {
                        string qId = ItemRegistry.QualifyItemId(ingrediantId);
                        if (qId == null)
                        {
                            if (
                                !TryMakeNeededFor_SpacecoreContextTag(
                                    newCache,
                                    spacecoreVAE,
                                    recipeId,
                                    recipe,
                                    ingrediantId,
                                    ref key,
                                    ref makeNeedForInfoGroup
                                )
                            )
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (
                                !TryMakeNeededFor_SpecificItem(
                                    newCache,
                                    qId,
                                    ingrediantId,
                                    ref key,
                                    ref makeNeedForInfoGroup
                                )
                            )
                            {
                                continue;
                            }
                        }
                    }
                    if (makeNeedForInfoGroup == null)
                        continue;
                    if (!neededForRecipe.TryGetValue(key, out NeededForInfoGroup? neededForGroup))
                    {
                        neededForGroup = makeNeedForInfoGroup();
                        if (neededForGroup == null)
                            continue;
                        neededForRecipe[key] = neededForGroup;
                    }
                    neededForGroup.NeededFor.Add(new(count, recipe, itemInfo));
                }
            }

            static bool TryMakeNeededFor_WildSeeds(
                Dictionary<string, ItemInfo> newCache,
                CraftingRecipe recipe,
                string ingrediantId,
                [NotNullWhen(true)] ref string? key,
                [NotNullWhen(true)] ref Func<NeededForInfoGroup?>? makeNeedForInfoGroup
            )
            {
                if (!newCache.TryGetValue("(O)495", out ItemInfo? ingredientInfo))
                    return false;
                key = $"{ModEntry.ModId}/wild_seeds";
                makeNeedForInfoGroup = () =>
                {
                    return new NeededForInfoGroup(
                        ingredientInfo,
                        ingrediantId,
                        recipe.getNameFromIndex(ingrediantId),
                        static (info, owned) =>
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
                        }
                    );
                };
                return true;
            }

            static bool TryMakeNeededFor_Category(
                Dictionary<string, ItemInfo> newCache,
                CraftingRecipe recipe,
                string ingrediantId,
                int ingredientNum,
                [NotNullWhen(true)] ref string? key,
                [NotNullWhen(true)] ref Func<NeededForInfoGroup?>? makeNeedForInfoGroup
            )
            {
                key = $"{ModEntry.ModId}/category_{ingredientNum}";
                makeNeedForInfoGroup = () =>
                {
                    ItemInfo? ingredientInfo = newCache.Values.FirstOrDefault(itemInfo =>
                        itemInfo.Datum.Category == ingredientNum
                    );
                    if (ingredientInfo == null)
                        return null;
                    string craftingDesc = recipe.getNameFromIndex(ingrediantId);
                    if (craftingDesc == "???")
                    {
                        craftingDesc = SObject.GetCategoryDisplayName(ingredientNum);
                    }
                    return new NeededForInfoGroup(
                        ingredientInfo,
                        ingrediantId,
                        craftingDesc,
                        static (info, owned) =>
                        {
                            int ownedCount = 0;
                            foreach ((string itemId, OwnedItemGroup group) in owned.OwnedGroups)
                            {
                                if (group.CountRepr.Category.ToString() == info.RawId)
                                    ownedCount += group.CountRepr.ReprStack;
                            }
                            return ownedCount;
                        }
                    );
                };
                return true;
            }

            static bool TryMakeNeededFor_SpecificItem(
                Dictionary<string, ItemInfo> newCache,
                string qId,
                string ingrediantId,
                [NotNullWhen(true)] ref string? key,
                [NotNullWhen(true)] ref Func<NeededForInfoGroup?>? makeNeedForInfoGroup
            )
            {
                if (!newCache.TryGetValue(qId, out ItemInfo? ingredientInfo))
                    return false;
                key = qId;
                makeNeedForInfoGroup = () =>
                {
                    return new NeededForInfoGroup(
                        ingredientInfo,
                        ingrediantId,
                        ingredientInfo.ReprItem.DisplayName,
                        static (info, owned) =>
                        {
                            if (
                                owned.OwnedGroups.TryGetValue(
                                    info.ReprInfo.Datum.QualifiedItemId,
                                    out OwnedItemGroup? group
                                )
                            )
                                return group.CountRepr.ReprStack;
                            return 0;
                        }
                    );
                };
                return true;
            }

            static bool TryMakeNeededFor_SpacecoreContextTag(
                Dictionary<string, ItemInfo> newCache,
                dynamic? spacecoreVAE,
                string recipeId,
                CraftingRecipe recipe,
                string ingrediantId,
                [NotNullWhen(true)] ref string? key,
                [NotNullWhen(true)] ref Func<NeededForInfoGroup?>? makeNeedForInfoGroup
            )
            {
                // can't move any of this into a static func rip
                if (spacecoreVAE == null)
                {
                    if (!MenuHandler.IsPreloading)
                        ModEntry.Log($"Invalid ingredient '{ingrediantId}' for recipe '{recipe.name}'.", LogLevel.Warn);
                    return false;
                }
                // spacecore recipe overrides?
                key = $"{ModEntry.ModId}/contexttag_{ingrediantId}";
                makeNeedForInfoGroup = () =>
                {
                    SDUISprite? reprIcon = null;
                    ItemInfo? ingredientInfo = null;
                    string craftingDesc = ingrediantId;
                    if (spacecoreVAE.ContainsKey(recipeId))
                    {
                        dynamic spacecoreVAERecipe = spacecoreVAE[recipeId];
                        dynamic? spacecoreVAEIngredient = null;
                        foreach (dynamic ing in spacecoreVAERecipe.Ingredients)
                        {
                            if (ing.Value == ingrediantId)
                            {
                                spacecoreVAEIngredient = ing;
                                break;
                            }
                        }
                        if (spacecoreVAEIngredient != null)
                        {
                            craftingDesc = TokenParser.ParseText(spacecoreVAEIngredient.OverrideText);
                            if (spacecoreVAEIngredient.OverrideTexturePath != null)
                            {
                                reprIcon = new(
                                    Game1.content.Load<Texture2D>(spacecoreVAEIngredient.OverrideTexturePath),
                                    spacecoreVAEIngredient.OverrideTextureRect
                                );
                                ingredientInfo = newCache.Values.First(); // weeds
                            }
                        }
                    }
                    if (ingredientInfo == null)
                    {
                        ingredientInfo = newCache.Values.FirstOrDefault(itemInfo =>
                            itemInfo.ReprItem.HasContextTag(ingrediantId)
                        );
                        if (ingredientInfo == null)
                        {
                            if (!MenuHandler.IsPreloading)
                                ModEntry.Log(
                                    $"Invalid ingredient '{ingrediantId}' for recipe '{recipe.name}'.",
                                    LogLevel.Warn
                                );
                            return null;
                        }
                    }
                    return new NeededForInfoGroup(
                        ingredientInfo,
                        ingrediantId,
                        craftingDesc,
                        static (info, owned) =>
                        {
                            int ownedCount = 0;
                            foreach ((string itemId, OwnedItemGroup group) in owned.OwnedGroups)
                            {
                                if (group.CountRepr.HasContextTag(info.RawId))
                                    ownedCount += group.CountRepr.ReprStack;
                            }
                            return ownedCount;
                        }
                    )
                    {
                        ReprIcon = reprIcon,
                    };
                };
                return true;
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
