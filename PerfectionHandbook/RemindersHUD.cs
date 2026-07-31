using System.ComponentModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
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
            ModEntry.Log($"Remove: {entry.EntryId}");
            RaisePropertyChanged(nameof(Reminders));
            if (reminders.Count == 0)
                RaisePropertyChanged(nameof(HasReminders));
        }
    }

    public void RemoveEntryDisplay(ReminderEntryDisplay? display)
    {
        if (display?.Entry != null)
            RemoveEntry(display.Entry);
    }

    internal ReminderEntry? GetEntry(string kind, string entryId, string fromMod)
    {
        return reminders.FirstOrDefault(en => en.FromMod == fromMod && en.Kind == kind && en.EntryId == entryId);
    }
}

public sealed class RemindersHUD
{
    private readonly Func<RemindersContext, IMenuController> makeMenuCtrl;
    internal readonly RemindersContext ctx;
    private IMenuController? menuCtrl = null;

    public RemindersHUD(Func<RemindersContext, IMenuController> makeMenuCtrl)
    {
        this.makeMenuCtrl = makeMenuCtrl;
        this.ctx = new();
        this.ctx.PropertyChanged += OnCtxPropertyChanged;
    }

    public void ToggleEntry(ReminderEntry entry) => ctx.ToggleEntry(entry);

    public bool HasEntry(ReminderEntry entry) => ctx.HasEntry(entry);

    public void RemoveEntry(ReminderEntry entry) => ctx.RemoveEntry(entry);

    public ReminderEntry GetOrCreateEntry(string kind, string entryId, string fromMod = ModEntry.ModId) =>
        ctx.GetEntry(kind, entryId, fromMod) ?? new ReminderEntry(kind, entryId, fromMod);

    private Point HUDPositionSelector()
    {
        if (menuCtrl == null)
            return Point.Zero;
        return ModEntry
            .config.RemindersHUDPosition.GetViewportPosition(new(menuCtrl.Menu.width, menuCtrl.Menu.height))
            .ToPoint();
    }

    internal void Reposition()
    {
        menuCtrl?.Reposition();
    }

    public void Activate()
    {
        if (menuCtrl != null)
            return;
        menuCtrl = makeMenuCtrl(ctx);
        menuCtrl.DimmingAmount = 0;
        menuCtrl.HideHUD = false;
        menuCtrl.OpenSound = string.Empty;
        menuCtrl.NavigateSound = string.Empty;
        menuCtrl.PositionSelector = HUDPositionSelector;
        menuCtrl.SetGutters(0, 0);
        Game1.onScreenMenus.Add(menuCtrl.Menu);
        Reposition();
    }

    public void Deactivate()
    {
        if (menuCtrl == null)
            return;
        Game1.onScreenMenus.Remove(menuCtrl.Menu);
        menuCtrl?.Dispose();
        menuCtrl = null;
    }

    public void ToggleVisibility()
    {
        if (menuCtrl != null)
            Deactivate();
        else
            Activate();
    }

    private void OnCtxPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RemindersContext.Reminders))
        {
            if (menuCtrl != null && ctx.reminders.Count == 0)
            {
                Deactivate();
            }
            else if (menuCtrl == null && ctx.reminders.Count > 0)
            {
                Activate();
            }
        }
    }

    #region tracking
    internal void SaveLoadedSetup(Farmer who)
    {
        who.basicShipped.OnValueAdded += BasicShippedOnValueAdded;
        who.basicShipped.OnValueTargetUpdated += BasicShippedOnValueTargetUpdated;
        who.recipesCooked.OnValueAdded += RecipesCookedOnValueAdded;
        who.craftingRecipes.OnValueAdded += CraftingRecipesOnValueAdded;
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
        if (value >= 1)
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
    private void CraftingRecipesOnValueAdded(string key, int value)
    {
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
