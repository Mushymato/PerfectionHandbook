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

public interface ILocationSourceInfo
{
    string LocationId { get; }
    GameLocation Location { get; }
}

public sealed record FishSourceInfo(
    string LocationId,
    GameLocation Location,
    SpawnFishData? Spawn,
    FishAreaData? FishArea
) : ILocationSourceInfo;

public sealed record ForageSourceInfo(string LocationId, GameLocation Location, SpawnForageData Spawn)
    : ILocationSourceInfo;

public sealed record FishAreaSourceInfo(string LocationId, GameLocation Location, FishAreaData Area)
    : ILocationSourceInfo;

public sealed record FishSpawnReq(
    bool? Rain,
    int MinFishing,
    IReadOnlyList<(int, int)> TimeRanges,
    IReadOnlyList<string>? CrabPotGroups
);

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
    public List<FishSourceInfo> FromFishing = [];
    public List<FishAreaSourceInfo> FromFishAreas = [];
    public FishSpawnReq? FishReq = null;
    public List<ForageSourceInfo> FromForage = [];

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
    private static readonly HashTracker hashLocation = new(
        nameof(Game1.locationData),
        static () => Game1.locationData.GetHashCode()
    );
    private static readonly InvalidateTracker invalFish = InvalidateTracker.GetInvalidateTracker("Data/Fish");

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
    public static IReadOnlyDictionary<string, ItemInfo> Cache
    {
        get
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
                ModEntry.Log($"ItemInfoCache: refreshed in {stopwatch.Elapsed}", LogLevel.Info);
            return cacheRet;
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

    private static void UpdateFromRecipes(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        bool cookingChanged = hashCooking.CheckChanged();
        bool craftingChanged = hashCrafting.CheckChanged();
        if (!cookingChanged && !craftingChanged && useCached)
            return;

        ModEntry.Log($"UpdateFromRecipes({useCached})");
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
        if (!hashCrop.CheckChanged() && useCached)
            return;
        ModEntry.Log($"UpdateFromCrop({useCached})");
        // when using prior cache, clear previous crop data
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

    private static void UpdateFromLocation(Dictionary<string, ItemInfo> cacheRet, bool useCached)
    {
        if (!hashLocation.CheckChanged() && useCached)
            return;
        if (useCached)
            foreach (ItemInfo itemInfo in cacheRet.Values)
            {
                itemInfo.FromFishing.Clear();
                itemInfo.FromForage.Clear();
            }
        ModEntry.Log($"UpdateFromLocation({useCached})");
        foreach ((string locationName, LocationData locationData) in Game1.locationData)
        {
            if (Game1.getLocationFromName(locationName) is not GameLocation location)
                continue;
            // fish
            foreach (SpawnFishData spawnFishData in locationData.Fish ?? [])
            {
                FishAreaData? fishAreaData = null;
                if (spawnFishData.Id != null)
                {
                    locationData.FishAreas.TryGetValue(spawnFishData.Id, out fishAreaData);
                }
                foreach (ParsedItemData parsedItemData in GameQueryHelper.SimplifiedResolveAll(spawnFishData, location))
                {
                    if (cacheRet.TryGetValue(parsedItemData.QualifiedItemId, out ItemInfo? itemInfo))
                    {
                        itemInfo.FromFishing.Add(new(locationName, location, spawnFishData, fishAreaData));
                    }
                }
            }
            // forage
            foreach (SpawnForageData spawnForageData in locationData.Forage ?? [])
            {
                foreach (
                    ParsedItemData parsedItemData in GameQueryHelper.SimplifiedResolveAll(spawnForageData, location)
                )
                {
                    if (cacheRet.TryGetValue(parsedItemData.QualifiedItemId, out ItemInfo? itemInfo))
                    {
                        itemInfo.FromForage.Add(new(locationName, location, spawnForageData));
                    }
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

            List<string>? crabPotsList = [];
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
