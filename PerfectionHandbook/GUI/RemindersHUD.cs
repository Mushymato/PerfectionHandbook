using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record ReminderEntry(string Kind, string EntryId, SDUISprite Icon, string Text, int Count)
{
    public bool IsSub { get; set; } = false;
    public readonly bool HasCount = Count > 1;
    public bool HasSubReminders => SubReminders != null;
    public IReadOnlyList<ReminderEntry>? SubReminders = null;
    public bool IsValid
    {
        get { return ItemRegistry.GetData(EntryId) != null; }
    }
}

public sealed class RemindersContext() : INotifyPropertyChanged
{
    public readonly List<ReminderEntry> reminders = [];
    public IEnumerable<ReminderEntry> Reminders
    {
        get
        {
            foreach (ReminderEntry entry in reminders)
            {
                yield return entry;
                if (entry.SubReminders != null)
                {
                    foreach (ReminderEntry subEntry in entry.SubReminders)
                    {
                        yield return subEntry;
                    }
                }
            }
        }
    }

    public bool HasReminders => reminders.Any();
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
    }

    public bool ToggleEntry(ReminderEntry entry)
    {
        // if not in list, add; if in list, remove
        bool added = false;
        if (reminders.RemoveWhere(en => en.Kind == entry.Kind && en.EntryId == entry.EntryId) == 0)
        {
            added = true;
            reminders.Insert(0, entry);
            if (reminders.Count > ModEntry.config.RemindersMaxCount)
                reminders.RemoveAt(reminders.Count - 1);
        }

        RaisePropertyChanged(nameof(Reminders));
        if (reminders.Count == (added ? 1 : 0))
            RaisePropertyChanged(nameof(HasReminders));
        if (added)
            ModEntry.Log($"Toggle: {entry.Kind} {entry.EntryId}");
        return added;
    }

    public bool HasEntryKey(string kind, string entryId)
    {
        return reminders.Any(en => en.Kind == kind && en.EntryId == entryId);
    }

    public void RemoveEntryKey(string kind, string entryId, Func<ReminderEntry, bool>? match)
    {
        if (reminders.RemoveWhere(en => en.Kind == kind && en.EntryId == entryId && (match == null || match(en))) > 0)
        {
            ModEntry.Log($"Remove: {kind} {entryId}");
            RaisePropertyChanged(nameof(Reminders));
            if (reminders.Count == 0)
                RaisePropertyChanged(nameof(HasReminders));
        }
    }
}

public sealed class RemindersHUD(Func<IViewDrawable> makeDrawable, int screenId)
{
    public const string CookingKind = "CookingRecipe";
    public const string CraftingKind = "CraftingRecipe";
    public const string FishingKind = "FishCaught";
    public const string ShippedKind = "ItemShipped";
    public const string DonateKind = "MuseumDonate";
    public const string RecipesIngredientKind = "RecipesIngredient";

    // private const string ReminderSaveKey = $"{ModEntry.ModId}/Reminders";
    private IViewDrawable? drawable = null;
    internal readonly RemindersContext ctx = new();

    public bool ToggleEntry(ReminderEntry entry) => ctx.ToggleEntry(entry);

    public bool HasEntryKey(string kind, string entryId) => ctx.HasEntryKey(kind, entryId);

    public void RemoveEntryKey(string kind, string entryId, Func<ReminderEntry, bool>? match = null) =>
        ctx.RemoveEntryKey(kind, entryId, match);

    public void Activate()
    {
        if (drawable != null)
            return;
        ModEntry.help.Events.Display.RenderedHud += OnRenderedHud;
        drawable = makeDrawable();
        drawable.MaxSize = new(256, Game1.viewport.Height);
        drawable.Context = ctx;
    }

    public void Deactivate()
    {
        if (drawable == null)
            return;
        ModEntry.help.Events.Display.RenderedHud -= OnRenderedHud;
        drawable.Dispose();
        drawable = null;
    }

    public void ToggleVisibility()
    {
        if (drawable != null)
            Deactivate();
        else
            Activate();
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (drawable == null)
        {
            ModEntry.help.Events.Display.RenderedHud -= OnRenderedHud;
            return;
        }
        if (Context.ScreenId != screenId)
            return;
        drawable.Draw(e.SpriteBatch, ModEntry.config.RemindersHUDPosition.GetViewportPosition(drawable.ActualSize));
    }

    #region tracking
    internal void SaveLoadedSetup(Farmer who)
    {
        who.basicShipped.OnValueAdded += BasicShippedOnValueAdded;
        who.basicShipped.OnValueTargetUpdated += BasicShippedOnValueTargetUpdated;
        who.recipesCooked.OnValueAdded += RecipesCookedOnValueAdded;
        who.craftingRecipes.OnValueTargetUpdated += CraftingRecipesOnValueTargetUpdated;
        who.fishCaught.OnValueAdded += FishCaughtOnValueAdded;
        Game1.netWorldState.Value.MuseumPieces.OnValueAdded += MuseumPiecesOnValueAdded;
    }

    // shipped
    private void BasicShippedOnValueAdded(string key, int value)
    {
        RemoveEntryKey(ShippedKind, key, (entry) => value >= entry.Count);
    }

    private void BasicShippedOnValueTargetUpdated(string key, int old_target_value, int new_target_value)
    {
        RemoveEntryKey(ShippedKind, key, (entry) => new_target_value >= entry.Count);
    }

    // cooking
    private void RecipesCookedOnValueAdded(string key, int value)
    {
        RemoveEntryKey(CookingKind, key);
    }

    // crafting
    private void CraftingRecipesOnValueTargetUpdated(string key, int old_target_value, int new_target_value)
    {
        if (old_target_value == 0 && new_target_value == 1)
            RemoveEntryKey(CraftingKind, key);
    }

    // fished
    private void FishCaughtOnValueAdded(string key, int[] value)
    {
        RemoveEntryKey(FishingKind, key);
    }

    // donated
    private void MuseumPiecesOnValueAdded(Vector2 key, string value)
    {
        RemoveEntryKey(DonateKind, value);
    }
    #endregion
}
