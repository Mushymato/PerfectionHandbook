using System.ComponentModel;
using StardewValley.Extensions;

namespace PerfectionHandbook.Reminders;

public sealed class RemindersContext() : INotifyPropertyChanged
{
    internal readonly List<ReminderEntry> reminders = [];
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
