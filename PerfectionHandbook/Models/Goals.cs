using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace PerfectionHandbook.Models;

public record GoalFulfillment(Farmer? Who, int Count, int Total)
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
    public bool HasMiniIcon => MiniIcon != null;
}

public interface IGoal
{
    bool IsShared { get; }
    string DisplayName { get; }
    ParsedItemData DisplayIcon { get; }
    GoalFulfillment GetFulfillment(Farmer who);
    object? GetPageContext(GoalContext goalCtx);
}

public interface IPerfectionGoal : IGoal
{
    float PercentWeight { get; }
}

public interface IAchievementGoal : IGoal
{
    int AchievementId { get; }
}

public static class Goals
{
    #region extensions
    public static GoalFulfillment GetBestFulfillment(
        this IGoal goal,
        Farmer who,
        GoalFulfillment? bestFulfillment = null
    )
    {
        bestFulfillment ??= goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return bestFulfillment;
        }
        foreach (Farmer otherFarmer in Game1.getAllFarmers())
        {
            if (otherFarmer == who || !otherFarmer.isCustomized.Value)
            {
                continue;
            }
            GoalFulfillment otherFulfillment = goal.GetFulfillment(otherFarmer);
            if (otherFulfillment.Percent > bestFulfillment.Percent)
            {
                bestFulfillment = otherFulfillment;
            }
        }
        return bestFulfillment;
    }
    #endregion

    #region defs
    public sealed class Perfection_ItemShipped : IPerfectionGoal
    {
        public float PercentWeight => 15f;
        public bool IsShared => false;

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Shipped");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)24");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ItemInfo sobj in ItemInfoCache.Cache.Values)
            {
                total++;
                if (sobj.IsPotentialShipped && who.basicShipped.ContainsKey(sobj.Datum.ItemId))
                    count++;
            }
            return new(who, count, total);
        }

        public object? GetPageContext(GoalContext goalCtx)
        {
            return new GoalItemShippedContext(goalCtx);
        }
    }

    public sealed class Perfection_ObelisksBuilt : IPerfectionGoal
    {
        public float PercentWeight => 4f;
        public bool IsShared => true;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Obelisks");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)688");

        // TODO: include modded obelisks
        public GoalFulfillment GetFulfillment(Farmer who) => new(null, Utility.GetObeliskTypesBuilt(), 4);
    }

    public sealed class Perfection_GoldClockBuilt : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => true;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_GoldClock");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)336");

        public GoalFulfillment GetFulfillment(Farmer who) =>
            new(null, Game1.IsBuildingConstructed("Gold Clock") ? 1 : 0, 1);
    }

    public sealed class Perfection_MonsterSlayered : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

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

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_GreatFriends");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)StardropTea");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (KeyValuePair<string, CharacterData> characterDatum in Game1.characterData)
            {
                string key = characterDatum.Key;
                CharacterData value = characterDatum.Value;
                if (!value.PerfectionScore || GameStateQuery.IsImmutablyFalse(value.CanSocialize))
                    continue;
                total++;
                if (who.friendshipData.TryGetValue(key, out Friendship? friendship))
                {
                    int maxPoints = (value.CanBeRomanced ? 8 : 10) * 250;
                    if (friendship != null && friendship.Points >= maxPoints)
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

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_FarmerLevel");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)PurpleBook");

        public GoalFulfillment GetFulfillment(Farmer who) => new(who, who.Level, 25);
    }

    public sealed class Perfection_StardropsFound : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

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

        public object? GetPageContext(GoalContext goalCtx)
        {
            return new GoalRecipesContext(goalCtx, true);
        }

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

        public object? GetPageContext(GoalContext goalCtx)
        {
            return new GoalRecipesContext(goalCtx, false);
        }

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

        public object? GetPageContext(GoalContext goalCtx)
        {
            return new GoalFishCaughtContext(goalCtx);
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_Fish");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)130");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                if (itemInfo.IsCatchableFish)
                {
                    total++;
                    if (who.fishCaught.ContainsKey(itemInfo.Datum.QualifiedItemId))
                    {
                        count++;
                    }
                }
            }
            return new(who, count, total);
        }
    }

    public sealed class Perfection_GoldenWalnutsFound : IPerfectionGoal
    {
        public float PercentWeight => 5f;
        public bool IsShared => true;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.content.LoadString("Strings\\UI:PT_GoldenWalnut");
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)73");

        public GoalFulfillment GetFulfillment(Farmer who) =>
            new(null, Game1.netWorldState.Value.GoldenWalnutsFound, 130);
    }

    public sealed class Achievement_Museum : IAchievementGoal
    {
        public int AchievementId => 5;
        public bool IsShared => true;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)587");

        public GoalFulfillment GetFulfillment(Farmer who) =>
            new(null, Game1.netWorldState.Value.MuseumPieces.Length, LibraryMuseum.totalArtifacts);
    }

    public sealed class Achievement_Polyculture : IAchievementGoal
    {
        public int AchievementId => 31;
        public bool IsShared => false;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)188");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (CropData value in Game1.cropData.Values)
            {
                if (value.CountForPolyculture)
                {
                    total++;
                    if (Game1.player.basicShipped.GetValueOrDefault(value.HarvestItemId, 0) >= 15)
                    {
                        count++;
                    }
                }
            }
            return new(null, count, total);
        }
    }

    public sealed class Achievement_Monoculture : IAchievementGoal
    {
        public int AchievementId => 32;
        public bool IsShared => false;

        public object? GetPageContext(GoalContext goalCtx)
        {
            return null;
        }

        public string DisplayName => Game1.achievements[AchievementId].Split('^').First();
        public ParsedItemData DisplayIcon => ItemRegistry.GetDataOrErrorItem("(O)258");

        public GoalFulfillment GetFulfillment(Farmer who)
        {
            int count = 0;
            int total = 300;
            string? notes = null;
            foreach (CropData value in Game1.cropData.Values)
            {
                if (value.CountForMonoculture)
                {
                    int shipped = Game1.player.basicShipped.GetValueOrDefault(value.HarvestItemId, 0);
                    if (shipped > count)
                    {
                        notes = ItemRegistry.GetDataOrErrorItem(value.HarvestItemId).DisplayName;
                        count = shipped;
                    }
                    if (count >= 300)
                        break;
                }
            }
            return new(null, count, total) { Notes = notes };
        }
    }
    #endregion

    #region perfection
    public static readonly Perfection_ItemShipped ItemShipped = new();
    public static readonly Perfection_ObelisksBuilt ObelisksBuilt = new();
    public static readonly Perfection_GoldClockBuilt GoldClockBuilt = new();
    public static readonly Perfection_MonsterSlayered MonsterSlayered = new();
    public static readonly Perfection_BestFriendsMade BestFriendsMade = new();
    public static readonly Perfection_SkillLeveled SkillLeveled = new();
    public static readonly Perfection_StardropsFound StardropsFound = new();
    public static readonly Perfection_RecipesCooked RecipesCooked = new();
    public static readonly Perfection_RecipesCrafted RecipesCrafted = new();
    public static readonly Perfection_FishCaught FishCaught = new();
    public static readonly Perfection_GoldenWalnutsFound GoldenWalnutsFound = new();
    public static List<IPerfectionGoal> PerfectionGoals =>
        [
            ItemShipped,
            ObelisksBuilt,
            GoldClockBuilt,
            MonsterSlayered,
            BestFriendsMade,
            SkillLeveled,
            StardropsFound,
            RecipesCrafted,
            RecipesCooked,
            FishCaught,
            GoldenWalnutsFound,
        ];
    #endregion

    #region achievements
    public static readonly Achievement_Museum Museum = new();
    public static readonly Achievement_Polyculture Polyculture = new();
    public static readonly Achievement_Monoculture Monoculture = new();
    public static readonly List<IAchievementGoal> AchievementGoals = [Museum, Polyculture, Monoculture];
    #endregion
}
