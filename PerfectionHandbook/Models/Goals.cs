using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace PerfectionHandbook.Models;

public record NumOutOfNum(int Count, int Total)
{
    public float Percent => Total > 0 ? (float)Count / Total : 0;
    public bool Filled => Count >= Total;
    public string? Notes { get; set; }
}

public interface IGoal
{
    bool IsShared { get; }
    NumOutOfNum CountAndTotal(Farmer who);
    object? PageContext { get; }
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
    public static KeyValuePair<Farmer, float> CheckPercent(this IGoal goal)
    {
        if (goal.IsShared)
        {
            return new(Game1.player, goal.CountAndTotal(Game1.player).Percent);
        }
        return Utility.GetFarmCompletion(who => goal.CountAndTotal(who).Percent);
    }
    #endregion

    #region defs
    public sealed class Perfection_ItemShipped : IPerfectionGoal
    {
        public float PercentWeight => 15f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                int category = allDatum.Category;
                if (
                    category != SObject.CookingCategory
                    && category != SObject.GemCategory
                    && SObject.isPotentialBasicShipped(allDatum.ItemId, allDatum.Category, allDatum.ObjectType)
                )
                {
                    total++;
                    if (who.basicShipped.ContainsKey(allDatum.ItemId))
                        count++;
                }
            }
            return new(count, total);
        }
    }

    public sealed class Perfection_ObelisksBuilt : IPerfectionGoal
    {
        public float PercentWeight => 4f;
        public bool IsShared => true;
        public object? PageContext => null;

        // TODO: include modded obelisks
        public NumOutOfNum CountAndTotal(Farmer who) => new(Utility.GetObeliskTypesBuilt(), 4);
    }

    public sealed class Perfection_GoldClockBuilt : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => true;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who) => new(Game1.IsBuildingConstructed("Gold Clock") ? 1 : 0, 1);
    }

    public sealed class Perfection_MonsterSlayered : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
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
                    killedNum += Game1.stats.getMonstersKilled(target);
                    if (killedNum >= value.Count)
                    {
                        count++;
                        break;
                    }
                }
                total++;
            }
            return new(count, total);
        }
    }

    public sealed class Perfection_BestFriendsMade : IPerfectionGoal
    {
        public float PercentWeight => 11f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
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
            return new(count, total);
        }
    }

    // TODO: spacecore skills
    public sealed class Perfection_SkillLeveled : IPerfectionGoal
    {
        public float PercentWeight => 5f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who) => new(who.Level, 25);
    }

    public sealed class Perfection_StardropsFound : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
        {
            return new(Utility.numStardropsFound(who), 7);
        }
    }

    public sealed class Perfection_RecipesCooked : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
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
            return new(count, total);
        }
    }

    public sealed class Perfection_RecipesCrafted : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
        {
            int count = 0;
            int total = 0;
            Dictionary<string, string> craftingRecipes = CraftingRecipe.craftingRecipes;
            foreach (string key in craftingRecipes.Keys)
            {
                if (key == "Wedding Ring")
                    continue;
                total++;
                if (who.craftingRecipes.TryGetValue(key, out var craftedNum) && craftedNum > 0)
                {
                    count++;
                }
            }
            return new(count, total);
        }
    }

    public sealed class Perfection_FishCaught : IPerfectionGoal
    {
        public float PercentWeight => 10f;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
        {
            int count = 0;
            int total = 0;
            foreach (ParsedItemData allDatum in ItemRegistry.GetObjectTypeDefinition().GetAllData())
            {
                if (
                    allDatum.ObjectType == "Fish"
                    && !(allDatum.RawData is ObjectData { ExcludeFromFishingCollection: not false })
                )
                {
                    total++;
                    if (who.fishCaught.ContainsKey(allDatum.QualifiedItemId))
                    {
                        count++;
                    }
                }
            }
            return new(count, total);
        }
    }

    public sealed class Perfection_GoldenWalnutsFound : IPerfectionGoal
    {
        public float PercentWeight => 5f;
        public bool IsShared => true;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who) => new(Game1.netWorldState.Value.GoldenWalnutsFound, 130);
    }

    public sealed class Achievement_Museum : IAchievementGoal
    {
        public int AchievementId => 5;
        public bool IsShared => true;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who) =>
            new(Game1.netWorldState.Value.MuseumPieces.Length, LibraryMuseum.totalArtifacts);
    }

    public sealed class Achievement_Polyculture : IAchievementGoal
    {
        public int AchievementId => 31;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
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
            return new(count, total);
        }
    }

    public sealed class Achievement_Monoculture : IAchievementGoal
    {
        public int AchievementId => 32;
        public bool IsShared => false;
        public object? PageContext => null;

        public NumOutOfNum CountAndTotal(Farmer who)
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
            return new(count, total) { Notes = notes };
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
    public static readonly List<IPerfectionGoal> PerfectionGoals =
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

    internal static void DebugPrintPerfection(string cmd, string[] args)
    {
        if (!Context.IsWorldReady)
            return;
        foreach (IPerfectionGoal goal in PerfectionGoals)
        {
            (Farmer farmer, float percent) = goal.CheckPercent();
            ModEntry.Log(
                $"{goal.GetType().Name}\tFarmer={farmer.displayName}\tPercent={percent:P2}\nWeighted={percent * goal.PercentWeight / 100f:P2}",
                LogLevel.Info
            );
        }
    }
    #endregion

    #region achievements
    public static readonly Achievement_Museum Museum = new();
    public static readonly Achievement_Polyculture Polyculture = new();
    public static readonly Achievement_Monoculture Monoculture = new();
    public static readonly List<IAchievementGoal> AchievementGoals = [Museum, Polyculture, Monoculture];

    internal static void DebugPrintAchievements(string cmd, string[] args)
    {
        if (!Context.IsWorldReady)
            return;
        foreach (IAchievementGoal goal in AchievementGoals)
        {
            (Farmer farmer, float percent) = goal.CheckPercent();
            ModEntry.Log($"{goal.GetType().Name}\tFarmer={farmer.displayName}\tPercent={percent:P2}", LogLevel.Info);
        }
    }
    #endregion
}
