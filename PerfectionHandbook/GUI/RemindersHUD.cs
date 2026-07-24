using System.ComponentModel;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record ReminderEntry(string Key, SDUISprite Icon, string Text, int Count)
{
    public bool IsSub { get; set; } = false;
    public readonly bool HasCount = Count > 1;
    public bool HasSubReminders => SubReminders != null;
    public IReadOnlyList<ReminderEntry>? SubReminders = null;
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
        if (reminders.RemoveWhere(en => en.Key == entry.Key) == 0)
        {
            added = true;
            reminders.Insert(0, entry);
            if (reminders.Count > ModEntry.config.RemindersMaxCount)
                reminders.RemoveAt(reminders.Count - 1);
        }

        RaisePropertyChanged(nameof(Reminders));
        if (reminders.Count == (added ? 1 : 0))
            RaisePropertyChanged(nameof(HasReminders));
        return added;
    }

    public bool HasEntryKey(string key)
    {
        return reminders.Any(en => en.Key == key);
    }
}

public sealed class RemindersHUD(Func<IViewDrawable> makeDrawable, int screenId)
{
    private IViewDrawable? drawable = null;
    internal readonly RemindersContext ctx = new();

    public bool ToggleEntry(ReminderEntry entry) => ctx.ToggleEntry(entry);

    internal bool HasEntryKey(string key) => ctx.HasEntryKey(key);

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
}
