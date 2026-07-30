using System.ComponentModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook;

public sealed class RemindersContext() : INotifyPropertyChanged
{
    public readonly List<ReminderEntry> reminders = [];
    public IEnumerable<ReminderEntryDisplay> Reminders
    {
        get
        {
            foreach (ReminderEntry entry in reminders)
            {
                if (entry.Display == null)
                    continue;
                yield return entry.Display;
                foreach (ReminderEntryDisplay display in entry.Display.CastedSubReminders)
                    yield return display;
            }
        }
    }

    public bool HasReminders => reminders.Any();
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
    }

    public void ToggleEntry(ReminderEntry entry)
    {
        // validate this is a displayable entry
        if (entry.Display == null)
            return;

        // if not in list, add; if in list, remove
        bool added = false;
        if (reminders.RemoveWhere(entry.SameAs) == 0)
        {
            added = true;
            reminders.Insert(0, entry);
            entry.Active = true;
            if (reminders.Count > ModEntry.config.RemindersMaxCount)
            {
                reminders[^1].Active = false;
                reminders.RemoveAt(reminders.Count - 1);
            }
        }
        else
        {
            entry.Active = false;
        }

        RaisePropertyChanged(nameof(Reminders));
        if (reminders.Count == (added ? 1 : 0))
            RaisePropertyChanged(nameof(HasReminders));
        return;
    }

    public bool HasEntry(ReminderEntry entry)
    {
        return reminders.Any(entry.SameAs);
    }

    public void RemoveEntry(ReminderEntry entry)
    {
        if (reminders.RemoveWhere(entry.SameAs) > 0)
        {
            entry.Active = false;
            ModEntry.Log($"Remove: {entry}");
            RaisePropertyChanged(nameof(Reminders));
            if (reminders.Count == 0)
                RaisePropertyChanged(nameof(HasReminders));
        }
    }

    internal ReminderEntry? GetEntry(string kind, string entryId, string fromMod)
    {
        return reminders.FirstOrDefault(en => en.FromMod == fromMod && en.Kind == kind && en.EntryId == entryId);
    }
}

public sealed class RemindersHUD
{
    private readonly Func<IViewDrawable> makeDrawable;
    private readonly int screenId;
    internal readonly RemindersContext ctx;
    private IViewDrawable? drawable = null;

    public RemindersHUD(Func<IViewDrawable> makeDrawable, int screenId)
    {
        this.makeDrawable = makeDrawable;
        this.screenId = screenId;
        this.ctx = new();
        this.ctx.PropertyChanged += OnCtxPropertyChanged;
    }

    public void ToggleEntry(ReminderEntry entry) => ctx.ToggleEntry(entry);

    public bool HasEntry(ReminderEntry entry) => ctx.HasEntry(entry);

    public void RemoveEntry(ReminderEntry entry) => ctx.RemoveEntry(entry);

    public ReminderEntry GetOrCreateEntry(string kind, string entryId, string fromMod = ModEntry.ModId) =>
        ctx.GetEntry(kind, entryId, fromMod) ?? new ReminderEntry(kind, entryId, fromMod);

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

    private void OnCtxPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RemindersContext.Reminders))
        {
            if (drawable != null && ctx.reminders.Count == 0)
            {
                Deactivate();
            }
            else if (drawable == null && ctx.reminders.Count > 0)
            {
                Activate();
            }
        }
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
        RemoveShippedReminders(key, value);
    }

    private void BasicShippedOnValueTargetUpdated(string key, int old_target_value, int new_target_value)
    {
        RemoveShippedReminders(key, new_target_value);
    }

    private void RemoveShippedReminders(string key, int value)
    {
        RemoveEntry(new(ReminderEntryFactory.Kind_ItemShipped, key));
        if (value >= ReminderEntryFactory.PolycultureCount)
            RemoveEntry(new(ReminderEntryFactory.Kind_ItemShippedPolyculture, key));
        if (value >= ReminderEntryFactory.MonocultureCount)
            RemoveEntry(new(ReminderEntryFactory.Kind_ItemShippedMonoculture, key));
    }

    // cooking
    private void RecipesCookedOnValueAdded(string key, int value)
    {
        RemoveEntry(new(ReminderEntryFactory.Kind_CookingRecipe, key));
    }

    // crafting
    private void CraftingRecipesOnValueTargetUpdated(string key, int old_target_value, int new_target_value)
    {
        if (old_target_value == 0 && new_target_value == 1)
            RemoveEntry(new(ReminderEntryFactory.Kind_CraftingRecipe, key));
    }

    // fished
    private void FishCaughtOnValueAdded(string key, int[] value)
    {
        RemoveEntry(new(ReminderEntryFactory.Kind_FishCaught, key));
    }

    // donated
    private void MuseumPiecesOnValueAdded(Vector2 key, string value)
    {
        RemoveEntry(new(ReminderEntryFactory.Kind_MuseumDonate, value));
    }
    #endregion
}
