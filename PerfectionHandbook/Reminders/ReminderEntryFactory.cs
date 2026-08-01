using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PerfectionHandbook.GUI;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Menus;

namespace PerfectionHandbook.Reminders;

public sealed record ReminderEntry(string Kind, string EntryId, string FromMod = ModEntry.ModId)
    : IReminderEntry,
        INotifyPropertyChanged
{
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

    internal ReminderEntryDisplay? Display =>
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
    public readonly bool HasCount = Count > 1;

    public bool IsSub { get; private set; } = false;
    public ReminderEntry? Entry { get; private set; } = null;

    [Notify]
    private bool showing = true;

    [Notify]
    private string displayText = Text;

    public string Transition => IsSub ? "500ms EaseOutCubic" : string.Empty;

    internal IList<ReminderEntryDisplay> CastedSubReminders =
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

        if (display.IsSub)
            display.Showing = ModEntry.config.RemindersDefaultExpanded;

        return display;
    }

    public void ToggleSubEntries()
    {
        if (IsSub || CastedSubReminders.Count == 0)
            return;
        bool state = false;
        foreach (ReminderEntryDisplay entryDisplay in CastedSubReminders)
        {
            entryDisplay.Showing = !entryDisplay.Showing;
            state = entryDisplay.Showing;
        }
        DisplayText = state ? Text : I18n.Ui_ReminderWithCount(CastedSubReminders.Count, Text);
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

    public const string Kind_RecipesIngredient = "RecipesIngredient";

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
                ObjectItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Ship)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_ItemShippedPolyculture,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ObjectItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Ship, PolycultureCount)
        );
        AddEntryMaker(
            ModEntry.ModId,
            Kind_ItemShippedMonoculture,
            static (entry, [NotNullWhen(true)] out entryDisplay) =>
                ObjectItemId_GetReminderEntryDisplay(entry, out entryDisplay, I18n.Reminder_Verb_Ship, MonocultureCount)
        );
        AddEntryMaker(ModEntry.ModId, Kind_RecipesIngredient, RecipesIngredient_GetReminderEntryDisplay);
        AddEntryMaker(ModEntry.ModId, Kind_CommunityCenterBundle, CommunityCenterBundle_GetReminderEntryDisplay);
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
                ? I18n.Reminder_Verb_Cook(itemInfo.ReprItem.DisplayName)
                : I18n.Reminder_Verb_Craft(itemInfo.ReprItem.DisplayName),
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
            getText(itemInfo.ReprItem.DisplayName),
            itemInfo.Datum.GetTexture(),
            itemInfo.Datum.GetSourceRect(),
            Count: count
        );
        return true;
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
            getText(itemInfo.ReprItem.DisplayName),
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
            I18n.Reminder_Verb_Prepare(neededForInfoGroup.ReprInfo.ReprItem.DisplayName),
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
                        ingredient.Info.ReprItem.DisplayName,
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
    #endregion
}
