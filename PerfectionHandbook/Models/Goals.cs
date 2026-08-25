using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace PerfectionHandbook.Models;

public sealed partial record GoalFulfillment(Farmer? Who, int Count, int Total) : IComparable<GoalFulfillment>
{
    public float Percent => Total > 0 ? (float)Count / Total : 0;
    public bool Filled => Count >= Total;
    public string? Notes { get; set; }
    public string DisplayText => I18n.Ui_Fulfillment_Dipslay(Count, Total);
    public string DiplayPercent => $"{Percent:P2}";
    public string TooltipText
    {
        get
        {
            string tooltip = I18n.Ui_Fulfillment_Tooltip(Count, Total, $"{Percent:P2}");
            if (Who != null)
                return string.Concat(Who.displayName, ": ", tooltip);
            return tooltip;
        }
    }
    public Texture2D MiniIcon => DrawHelper.GetFarmerMiniIcon(Who) ?? Game1.staminaRect;
    public bool HasMiniIcon => Who != null && MiniIcon != null;

    [Notify]
    private bool selected = false;
    public Color DisplayTint => Selected ? Color.White : Color.Transparent;

    // default reverse compare
    public int CompareTo(GoalFulfillment? other)
    {
        if (other == null)
            return -1;
        return other.Percent.CompareTo(Percent);
    }
}

public interface IGoal
{
    bool IsShared { get; }
    string DisplayName { get; }
    ParsedItemData DisplayIcon { get; }
    GoalFulfillment GetFulfillment(Farmer who);
    IPageContext? GetPageContext(GoalContext goalCtx);
}

public interface IPerfectionGoal : IGoal
{
    float PercentWeight { get; }
}

public static class Goals
{
    #region perfection
    public sealed class Perfection_ItemShipped : IPerfectionGoal
    {
        public float PercentWeight => 15f;
        public bool IsShared => false;

        public string DisplayName => I18n.Ui_Goals_ItemShipped();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)24");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ItemInfo sobj in ItemInfoCache.Cache.Values)
            {
                if (!sobj.IsPotentialShipped)
                    continue;
                total++;
                if (who.basicShipped.ContainsKey(sobj.Datum.ItemId))
                    count++;
            }
            return new(who, count, total);
        }

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalItemShippedContext(goalCtx);
    }

    public sealed class Perfection_BuildingsConstructed : IPerfectionGoal
    {
        public float PercentWeight =>
            Math.Min(Utility.GetObeliskTypesBuilt(), 4) * 1f + (Game1.IsBuildingConstructed("Gold Clock") ? 10 : 0);
        public bool IsShared => true;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalBuildingsConstructedContext(goalCtx);

        public string DisplayName => I18n.Ui_Goals_BuildingsConstructed();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)688");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            return new(
                null,
                Math.Min(Utility.GetObeliskTypesBuilt(), 4) + (Game1.IsBuildingConstructed("Gold Clock") ? 1 : 0),
                5
            );
        }
    }

    public sealed class Perfection_MonsterSlayered : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalMonsterSlayerContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_MonsterSlayer");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)767");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (MonsterSlayerQuestData value in DataLoader.MonsterSlayerQuests(Game1.content).Values)
            {
                int killedNum = 0;
                if (value.Targets == null)
                    continue;
                foreach (string target in value.Targets)
                {
                    killedNum += who.stats.getMonstersKilled(target);
                    if (killedNum >= value.Count)
                    {
                        count++;
                        break;
                    }
                }
                total++;
            }
            return new(who, count, total);
        }
    }

    public sealed class Perfection_BestFriendsMade : IPerfectionGoal
    {
        public float PercentWeight => 11f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalFriendsMadeContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_GreatFriends");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)StardropTea");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (NPCInfo npcInfo in NPCInfoCache.Cache.Values)
            {
                if (!npcInfo.CountForPerfection)
                    continue;
                total++;
                if (who.friendshipData.TryGetValue(npcInfo.Name, out Friendship? friendship))
                {
                    if (friendship != null && friendship.Points >= npcInfo.MaxPoints)
                        count++;
                }
            }
            return new(who, count, total);
        }
    }

    // TODO: spacecore skills
    public sealed class Perfection_SkillLeveled : IPerfectionGoal
    {
        public float PercentWeight => 5f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalSkillLeveledContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_FarmerLevel");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)PurpleBook");

        public GoalFulfillment GetFulfillment(Farmer who) => new(who, who.Level, 25);
    }

    public sealed class Perfection_StardropsFound : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalStardropsFoundContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Stardrops");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)434");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            return new(who, Utility.numStardropsFound(who), 7);
        }
    }

    public sealed class Perfection_RecipesCooked : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalRecipesContext(goalCtx, true);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Cooking");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)201");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            Dictionary<string, string> cookingRecipes = CraftingRecipe.cookingRecipes;
            foreach (KeyValuePair<string, string> item in cookingRecipes)
            {
                total++;
                string key = item.Key;
                if (who.cookingRecipes.ContainsKey(key))
                {
                    string key2 = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(item.Value.Split('/'), 2), 0);
                    if (who.recipesCooked.ContainsKey(key2))
                    {
                        count++;
                    }
                }
            }
            return new(who, count, total);
        }
    }

    public sealed class Perfection_RecipesCrafted : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalRecipesContext(goalCtx, false);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Crafting");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)621");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (string key in CraftingRecipe.craftingRecipes.Keys)
            {
                if (key == "Wedding Ring")
                    continue;
                total++;
                if (who.craftingRecipes.TryGetValue(key, out var craftedNum) && craftedNum > 0)
                {
                    count++;
                }
            }
            return new(who, count, total);
        }
    }

    public sealed class Perfection_FishCaught : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalFishCaughtContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Fish");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)130");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                if (!itemInfo.IsCatchableFish)
                    continue;
                total++;
                if (who.fishCaught.ContainsKey(itemInfo.Datum.QualifiedItemId))
                {
                    count++;
                }
            }
            return new(who, count, total);
        }
    }

    public sealed class Perfection_GoldenWalnutsFound : IPerfectionGoal
    {
        public float PercentWeight => 5f;
        public bool IsShared => true;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalGoldenWalnutsFoundContext(goalCtx);

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_GoldenWalnut");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)73");

        public GoalFulfillment GetFulfillment(Farmer who) =>
            new(null, Game1.netWorldState.Value.GoldenWalnutsFound, 130);
    }

    public static readonly List<IPerfectionGoal> PerfectionGoals =
    [
        new Perfection_ItemShipped(),
        new Perfection_RecipesCooked(),
        new Perfection_RecipesCrafted(),
        new Perfection_FishCaught(),
        new Perfection_MonsterSlayered(),
        new Perfection_BestFriendsMade(),
        new Perfection_SkillLeveled(),
        new Perfection_BuildingsConstructed(),
        new Perfection_StardropsFound(),
        new Perfection_GoldenWalnutsFound(),
    ];
    #endregion

    #region achievements
    public sealed class Achievement_CommunityCenter : IGoal
    {
        public bool IsShared => true;

        // Consider a save Joja only if they completely finish CC with Joja Development Form and seen the event
        private static bool IsJoja =>
            Game1.MasterPlayer != null
            && !Game1.MasterPlayer.eventsSeen.Contains("191393")
            && Game1.MasterPlayer.eventsSeen.Contains("502261");

        public string DisplayName => Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117");

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalCommunityCenterContext(goalCtx);

        public ParsedItemData DisplayIcon => IsJoja ? ItemRegistry.GetData("(O)167") : ItemRegistry.GetData("(F)1760");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            if (IsJoja)
            {
                return new GoalFulfillment(null, 5, 5);
            }
            int count = 0;
            int total = 0;
            foreach ((string bundleKey, string bundleData) in Game1.netWorldState.Value.BundleData)
            {
                int bundleId = Convert.ToInt32(bundleKey.Split('/')[1]);
                if (!Game1.netWorldState.Value.Bundles.TryGetValue(bundleId, out bool[] completion))
                {
                    continue;
                }
                // creating this to steal some parsing code
                Bundle bundle = new(bundleId, bundleData, completion, Point.Zero, "LooseSprites\\JunimoNote", null);
                total++;
                if (bundle.complete)
                {
                    count++;
                }
            }
            return new GoalFulfillment(null, count, total);
        }
    }

    public sealed class Achievement_Museum : IGoal
    {
        public readonly int AchievementId = 5;
        public bool IsShared => true;

        public IPageContext? GetPageContext(GoalContext goalCtx) => new GoalMuseumDonateContext(goalCtx);

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)587");

        public GoalFulfillment GetFulfillment(Farmer who) =>
            new(null, Game1.netWorldState.Value.MuseumPieces.Length, LibraryMuseum.totalArtifacts);
    }

    public sealed class Achievement_Polyculture : IGoal
    {
        public readonly int AchievementId = 31;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) =>
            new GoalCropListContext(goalCtx, CropListKind.Polyculture);

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)188");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                if (itemInfo.CountForPolyculture)
                {
                    total++;
                    if (who.basicShipped.GetValueOrDefault(itemInfo.Datum.ItemId, 0) >= 15)
                    {
                        count++;
                    }
                }
            }
            return new(who, count, total);
        }
    }

    public sealed class Achievement_Monoculture : IGoal
    {
        public readonly int AchievementId = 32;
        public bool IsShared => false;

        public IPageContext? GetPageContext(GoalContext goalCtx) =>
            new GoalCropListContext(goalCtx, CropListKind.Monoculture);

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)258");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 300;
            string? notes = null;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                if (itemInfo.CountForMonoculture)
                {
                    int shipped = who.basicShipped.GetValueOrDefault(itemInfo.Datum.ItemId, 0);
                    if (shipped > count)
                    {
                        notes = itemInfo.Datum.DisplayName;
                        count = shipped;
                    }
                    if (count >= 300)
                        break;
                }
            }
            return new(who, count, total) { Notes = notes };
        }
    }

    public static readonly List<IGoal> AchievementGoals =
    [
        new Achievement_CommunityCenter(),
        new Achievement_Museum(),
        new Achievement_Polyculture(),
        new Achievement_Monoculture(),
    ];
    #endregion
}
