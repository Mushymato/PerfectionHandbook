using System.ComponentModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
using StardewValley;

namespace PerfectionHandbook;

public sealed class RemindersHUD
{
    private const string ModDataReminders = $"{ModEntry.ModId}/Reminders";

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

    public void ToggleEntryKeyChecked(ReminderEntry? entry)
    {
        if (ModEntry.config.RemindersEditModifierKey.IsDown())
        {
            if (entry == null)
                return;
            MenuHandler.Reminders.ToggleEntry(entry);
        }
    }

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
        menuCtrl.ShowMouse = false;
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
        if (e.PropertyName == nameof(RemindersContext.HasReminders))
        {
            if (menuCtrl != null && !ctx.HasReminders)
            {
                Deactivate();
            }
            else if (menuCtrl == null && ctx.HasReminders)
            {
                Activate();
            }
        }
    }

    #region tracking
    internal void SaveLoaded(Farmer who)
    {
        who.basicShipped.OnValueAdded += BasicShippedOnValueAdded;
        who.basicShipped.OnValueTargetUpdated += BasicShippedOnValueTargetUpdated;
        who.recipesCooked.OnValueAdded += RecipesCookedOnValueAdded;
        who.craftingRecipes.OnValueAdded += CraftingRecipesOnValueAdded;
        who.fishCaught.OnValueAdded += FishCaughtOnValueAdded;

        Game1.netWorldState.Value.MuseumPieces.OnValueAdded += MuseumPiecesOnValueAdded;

        ctx.Reminders.Clear();
        if (
            who.modData.TryGetValue(ModDataReminders, out string reminderStr)
            && JsonConvert.DeserializeObject<List<ReminderEntry>>(reminderStr) is List<ReminderEntry> savedReminders
        )
        {
            foreach (ReminderEntry entry in savedReminders)
            {
                ctx.AddEntry(entry);
            }
            if (ctx.Reminders.Count > 0)
            {
                Activate();
            }
        }
    }

    internal void Saving(Farmer who)
    {
        ModEntry.Log($"Saving {ctx.Reminders.Count} reminders");
        who.modData[ModDataReminders] = JsonConvert.SerializeObject(ctx.Reminders);
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
