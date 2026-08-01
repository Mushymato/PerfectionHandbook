using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;
using StardewValley.Extensions;

namespace PerfectionHandbook.Reminders;

public sealed partial class RemindersContext()
{
    public readonly ObservableCollection<ReminderEntry> Reminders = [];

    [Notify]
    public bool hasReminders = false;

    public void ToggleEntry(ReminderEntry entry)
    {
        // validate this is a displayable entry
        if (entry.Display == null)
            return;

        // if not in list, add; if in list, remove
        if (Reminders.RemoveWhere(entry.SameAs) == 0)
        {
            Reminders.Insert(0, entry);
            entry.Active = true;
            if (Reminders.Count > ModEntry.config.RemindersMaxCount)
            {
                Reminders[^1].Active = false;
                Reminders.RemoveAt(Reminders.Count - 1);
            }
        }
        else
        {
            entry.Active = false;
        }

        HasReminders = Reminders.Count > 0;
        return;
    }

    public bool HasEntry(ReminderEntry entry)
    {
        return Reminders.Any(entry.SameAs);
    }

    public void RemoveEntry(ReminderEntry entry)
    {
        if (Reminders.RemoveWhere(entry.SameAs) > 0)
        {
            entry.Active = false;
            HasReminders = Reminders.Count > 0;
        }
    }

    public void AddEntry(ReminderEntry entry)
    {
        if (entry.Display != null)
        {
            Reminders.Add(entry);
            entry.Active = true;
            HasReminders = Reminders.Count > 0;
        }
    }

    public bool RemoveEntryDisplay(ReminderEntryDisplay? display)
    {
        if (display?.Entry != null)
        {
            RemoveEntry(display.Entry);
            return true;
        }
        return false;
    }

    internal ReminderEntry? GetEntry(string kind, string entryId, string fromMod)
    {
        return Reminders.FirstOrDefault(en => en.FromMod == fromMod && en.Kind == kind && en.EntryId == entryId);
    }
}
