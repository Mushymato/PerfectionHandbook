using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.Reminders;

public sealed record ReminderEntry(string Kind, string EntryId, string FromMod = ModEntry.ModId)
    : IReminderEntry,
        INotifyPropertyChanged
{
    public override int GetHashCode() => HashCode.Combine(Kind, EntryId, FromMod);

    public event EventHandler<bool>? ActiveStatusChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonIgnore]
    public bool Active
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, new(nameof(Active)));
                ActiveStatusChanged?.Invoke(this, field);
            }
        }
    }

    [JsonIgnore]
    public ReminderEntryDisplay? Display =>
        field ??= ReminderEntryFactory.TryCreate(this, out IReminderEntryDisplay? entryDisplay)
            ? ReminderEntryDisplay.FromInterface(entryDisplay, this, false)
            : null;

    public bool SameAs(ReminderEntry otherEntry)
    {
        return FromMod == otherEntry.FromMod && Kind == otherEntry.Kind && EntryId == otherEntry.EntryId;
    }
}

public sealed partial record ReminderEntryDisplay(
    string Text,
    Texture2D Texture,
    Rectangle SourceRect,
    int Count = 1,
    IEnumerable<IReminderEntryDisplay>? SubReminders = null
) : IReminderEntryDisplay
{
    public readonly SDUISprite Icon = new(Texture, SourceRect);

    public bool IsSub { get; private set; } = false;

    public ReminderEntry? Entry { get; private set; } = null;

    [Notify]
    private bool subDisplayed = true;

    [Notify]
    private string displayText = Text;

    [Notify]
    private int displayCount = Count;

    public bool HasCount => DisplayCount > 1;

    public IReadOnlyList<ReminderEntryDisplay> CastedSubReminders =
        SubReminders?.Select(subEntry => FromInterface(subEntry, null, true)).ToList() ?? [];

    internal static ReminderEntryDisplay FromInterface(
        IReminderEntryDisplay interfaceEntry,
        ReminderEntry? entry,
        bool isSub
    )
    {
        ReminderEntryDisplay display =
            interfaceEntry as ReminderEntryDisplay
            ?? new ReminderEntryDisplay(
                Text: interfaceEntry.Text,
                Texture: interfaceEntry.Texture,
                SourceRect: interfaceEntry.SourceRect,
                Count: interfaceEntry.Count,
                SubReminders: interfaceEntry.SubReminders
            );

        display.IsSub = isSub;
        display.Entry = entry;

        if (!display.IsSub)
            display.SubDisplayed = ModEntry.config.RemindersDefaultExpanded;

        return display;
    }

    public void ToggleSubEntries()
    {
        if (IsSub || CastedSubReminders.Count == 0)
            return;
        SubDisplayed = !SubDisplayed;
        DisplayText = SubDisplayed ? Text : I18n.Ui_ReminderWithCount(CastedSubReminders.Count, Text);
    }
}

public static class ReminderEntryFactory
{
    public const string Kind_CookingRecipe = "CookingRecipe";
    public const string Kind_CraftingRecipe = "CraftingRecipe";
    public const string Kind_FishCaught = "FishCaught";
    public const string Kind_MuseumDonate = "MuseumDonate";
    public const string Kind_ItemShipped = "ItemShipped";
    public const string Kind_ItemShippedPolyculture = "ItemShippedPolyculture";
    public const string Kind_ItemShippedMonoculture = "ItemShippedMonoculture";
    public const string Kind_CommunityCenterBundle = "CommunityCenterBundle";
    public const string Kind_BuildingsConstructed = "BuildingsConstructed";
    public const string Kind_MonsterSlayer = "MonsterSlayer";
    public const string Kind_FriendsMade = "FriendsMade";
    public const string Kind_GoldenWalnutsFound = "GoldenWalnutsFound";
    public const string Kind_SkillLeveled = "SkillLeveled";
    public const string Kind_Stardrops = "Stardrops";

    public const string Kind_RecipesIngredient = "RecipesIngredient";
    public const string Kind_Custom = "Custom";

    public const int PolycultureCount = 15;
    public const int MonocultureCount = 300;

    private static readonly Dictionary<(string, string), TryMakeReminderEntryDisplay> reminderEntryMakers = [];

    public static void Register()
    {
        AddEntryMaker(
            ModEntry.ModId,
            Kind_CookingRecipe,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                Recipe_GetReminderEntryDisplay(entry, true, out entryDisplay)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_CraftingRecipe,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                Recipe_GetReminderEntryDisplay(entry, false, out entryDisplay)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_FishCaught,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                QualifiedItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Fish)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_MuseumDonate,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ObjectItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Donate)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_ItemShipped,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ShippedObjectItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Ship, 1)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_ItemShippedPolyculture,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ShippedObjectItemId_GetReminderEntryDisplay(
                    entry,
                    out entryDisplay,
                    I18n.Reminder_Verb_Ship,
                    PolycultureCount
                )
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_ItemShippedMonoculture,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ShippedObjectItemId_GetReminderEntryDisplay(
                    entry,
                    out entryDisplay,
                    I18n.Reminder_Verb_Ship,
                    MonocultureCount
                )
        );
        AddEntryMaker(ModEntry.ModId, Kind_RecipesIngredient, RecipesIngredient_GetReminderEntryDisplay);
        AddEntryMaker(ModEntry.ModId, Kind_CommunityCenterBundle, CommunityCenterBundle_GetReminderEntryDisplay);
        AddEntryMaker(ModEntry.ModId, Kind_BuildingsConstructed, BuildingsConstructed_GetReminderEntryDisplay);
        AddEntryMaker(ModEntry.ModId, Kind_MonsterSlayer, MonsterSlayer_GetReminderEntryDisplay);
    }

    public static void AddEntryMaker(string modId, string kind, TryMakeReminderEntryDisplay makeDisplay)
    {
        reminderEntryMakers[(modId, kind)] = makeDisplay;
    }

    public static bool TryCreate(
        ReminderEntry entry,
        [NotNullWhen(true)] out IReminderEntryDisplay? reminderEntryDisplay
    )
    {
        reminderEntryDisplay = null;

        if (
            reminderEntryMakers.TryGetValue((entry.FromMod, entry.Kind), out TryMakeReminderEntryDisplay? makeDisplay)
            && makeDisplay(entry.EntryId, out IReminderEntryDisplay? entryDisplay)
        )
        {
            reminderEntryDisplay = entryDisplay;
            return reminderEntryDisplay != null;
        }

        return false;
    }

    #region factory methods
    public static bool Recipe_GetReminderEntryDisplay(
        string entryId,
        bool isCooking,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
    )
    {
        entryDisplay = null;
        if (!(isCooking ? CraftingRecipe.cookingRecipes : CraftingRecipe.craftingRecipes).ContainsKey(entryId))
        {
            return false;
        }
        CraftingRecipe recipe = ItemInfoCache.MakeCraftingRecipe(entryId, isCooking);
        if (recipe.createItem()?.QualifiedItemId is not string qId) // must do this to account for spacecore
        {
            return false;
        }
        if (!ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? itemInfo))
        {
            return false;
        }
        if (
            itemInfo.FromRecipe.FirstOrDefault(recipeWithNeeds =>
                recipeWithNeeds.Recipe.isCookingRecipe == isCooking && recipeWithNeeds.Recipe.name == recipe.name
            )
            is not CraftingRecipeWithNeeds craftingRecipeWithNeeds
        )
        {
            return false;
        }

        List<IReminderEntryDisplay> subReminders = [];
        foreach ((NeededForInfoGroup group, NeededForInfo need) in craftingRecipeWithNeeds.Needs)
        {
            subReminders.Add(
                new ReminderEntryDisplay(
                    group.CraftingDesc,
                    group.ReprInfo.Datum.GetTexture(),
                    group.ReprInfo.Datum.GetSourceRect(),
                    need.Count
                )
            );
        }
        entryDisplay = new ReminderEntryDisplay(
            isCooking
                ? I18n.Reminder_Verb_Cook(itemInfo.Datum.DisplayName)
                : I18n.Reminder_Verb_Craft(itemInfo.Datum.DisplayName),
            itemInfo.Datum.GetTexture(),
            itemInfo.Datum.GetSourceRect(),
            recipe.numberProducedPerCraft,
            subReminders
        );
        return true;
    }

    private static bool QualifiedItemId_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay,
        Func<string, string> getText,
        int count = 1
    )
    {
        entryDisplay = null;
        if (!ItemInfoCache.Cache.TryGetValue(entryId, out ItemInfo? itemInfo))
        {
            return false;
        }
        entryDisplay = new ReminderEntryDisplay(
            getText(itemInfo.Datum.DisplayName),
            itemInfo.Datum.GetTexture(),
            itemInfo.Datum.GetSourceRect(),
            Count: count
        );
        return true;
    }

    private static bool ShippedObjectItemId_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay,
        Func<string, string> getText,
        int count
    )
    {
        return ObjectItemId_GetReminderEntryDisplay(
            entryId,
            out entryDisplay,
            getText,
            count - Game1.player.basicShipped.GetValueOrDefault(entryId, 0)
        );
    }

    private static bool ObjectItemId_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay,
        Func<string, string> getText,
        int count = 1
    )
    {
        entryDisplay = null;
        if (!ItemInfoCache.Cache.TryGetValue(string.Concat("(O)", entryId), out ItemInfo? itemInfo))
        {
            return false;
        }
        entryDisplay = new ReminderEntryDisplay(
            getText(itemInfo.Datum.DisplayName),
            itemInfo.Datum.GetTexture(),
            itemInfo.Datum.GetSourceRect(),
            Count: count
        );
        return true;
    }

    private static bool RecipesIngredient_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
    )
    {
        entryDisplay = null;
        if (!ItemInfoCache.NeededForRecipe.TryGetValue(entryId, out NeededForInfoGroup? neededForInfoGroup))
        {
            return false;
        }
        entryDisplay = new ReminderEntryDisplay(
            I18n.Reminder_Verb_Prepare(neededForInfoGroup.ReprInfo.Datum.DisplayName),
            neededForInfoGroup.ReprInfo.Datum.GetTexture(),
            neededForInfoGroup.ReprInfo.Datum.GetSourceRect(),
            Count: neededForInfoGroup.GetNotYetCrafted(Game1.player).Sum(notYet => notYet.Count)
        );
        return true;
    }

    private static bool CommunityCenterBundle_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
    )
    {
        entryDisplay = null;
        if (!Game1.netWorldState.Value.BundleData.TryGetValue(entryId, out string? bundleData))
        {
            return false;
        }
        int bundleId = Convert.ToInt32(entryId.Split('/')[1]);
        if (!Game1.netWorldState.Value.Bundles.TryGetValue(bundleId, out bool[] completion))
        {
            return false;
        }
        Bundle bundle = new(bundleId, bundleData, completion, Point.Zero, "LooseSprites\\JunimoNote", null);

        List<IReminderEntryDisplay> subReminders = [];
        foreach (CommunityCenterBundleIngredient ingredient in GoalCommunityCenterContext.GetBundleIngredients(bundle))
        {
            if (!ingredient.Complete)
            {
                subReminders.Add(
                    new ReminderEntryDisplay(
                        ingredient.Info.Datum.DisplayName,
                        ingredient.Info.Datum.GetTexture(),
                        ingredient.Info.Datum.GetSourceRect(),
                        ingredient.Count
                    )
                );
            }
        }

        SDUISprite sprite = GoalCommunityCenterContext.GetBundleTexture(bundle);
        entryDisplay = new ReminderEntryDisplay(
            I18n.Reminder_Verb_Bundle(bundle.label),
            sprite.Texture,
            sprite.SourceRect ?? sprite.Texture.Bounds,
            SubReminders: subReminders
        );
        return true;
    }

    private static bool BuildingsConstructed_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
    )
    {
        entryDisplay = null;
        if (!Game1.buildingData.TryGetValue(entryId, out BuildingData? buildingData))
        {
            return false;
        }
        ParsedItemData goldCoin = ItemRegistry.GetDataOrErrorItem("(O)GoldCoin");
        List<IReminderEntryDisplay> subReminders =
        [
            new ReminderEntryDisplay(
                buildingData.BuildCost.ToString(),
                goldCoin.GetTexture(),
                goldCoin.GetSourceRect()
            ),
        ];
        if (buildingData.BuildMaterials != null)
        {
            foreach (BuildingMaterial material in buildingData.BuildMaterials)
            {
                if (ItemInfoCache.Cache.TryGetValue(material.ItemId, out ItemInfo? itemInfo))
                {
                    subReminders.Add(
                        new ReminderEntryDisplay(
                            itemInfo.Datum.DisplayName,
                            itemInfo.Datum.GetTexture(),
                            itemInfo.Datum.GetSourceRect(),
                            material.Amount
                        )
                    );
                }
            }
        }

        Texture2D buildingTx = DrawHelper.SafeLoad(buildingData.Texture);
        entryDisplay = new ReminderEntryDisplay(
            I18n.Reminder_Verb_Build(TokenParser.ParseText(buildingData.Name) ?? entryId),
            buildingTx,
            buildingData.SourceRect.IsEmpty ? buildingTx.Bounds : buildingData.SourceRect,
            SubReminders: subReminders
        );
        return true;
    }

    private static bool MonsterSlayer_GetReminderEntryDisplay(
        string entryId,
        [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
    )
    {
        entryDisplay = null;

        if (
            !DataLoader
                .MonsterSlayerQuests(Game1.content)
                .TryGetValue(entryId, out MonsterSlayerQuestData? slayerQuestData)
        )
        {
            return false;
        }
        if (slayerQuestData.Targets == null || slayerQuestData.Targets.Count == 0)
        {
            return false;
        }

        SDUISprite displaySprite = GoalMonsterSlayerContext.GetMonsterDisplaySprite(slayerQuestData);

        entryDisplay = new ReminderEntryDisplay(
            TokenParser.ParseText(slayerQuestData.DisplayName) ?? entryId,
            displaySprite.Texture,
            displaySprite.SourceRect ?? displaySprite.Texture.Bounds,
            Count: slayerQuestData.Count
                - slayerQuestData.Targets.Sum(target =>
                {
                    if (Game1.player.stats.specificMonstersKilled.TryGetValue(target, out int count))
                        return count;
                    return 0;
                })
        );

        return true;
    }
    #endregion
}
