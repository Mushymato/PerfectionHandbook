using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;
using StardewValley.Extensions;

namespace PerfectionHandbook.Reminders;

public sealed partial class RemindersContext()
{
    public readonly ObservableCollection<ReminderEntry> ReminderEntries = [];

    [Notify]
    public bool hasReminders = false;

    public void ToggleEntry(ReminderEntry entry)
    {
        // validate this is a displayable entry
        if (entry.Display == null)
            return;

        // if not in list, add; if in list, remove
        if (ReminderEntries.RemoveWhere(entry.SameAs) == 0)
        {
            ReminderEntries.Insert(0, entry);
            entry.Active = true;
            if (ReminderEntries.Count > ModEntry.config.RemindersMaxCount)
            {
                ReminderEntries[^1].Active = false;
                ReminderEntries.RemoveAt(ReminderEntries.Count - 1);
            }
        }
        else
        {
            entry.Active = false;
        }

        HasReminders = ReminderEntries.Count > 0;
        return;
    }

    public bool HasEntry(ReminderEntry entry)
    {
        return ReminderEntries.Any(entry.SameAs);
    }

    public void RemoveEntry(ReminderEntry entry)
    {
        if (ReminderEntries.RemoveWhere(entry.SameAs) > 0)
        {
            entry.Active = false;
            HasReminders = ReminderEntries.Count > 0;
        }
    }

    public void AddEntry(ReminderEntry entry)
    {
        if (entry.Display != null)
        {
            ReminderEntries.Add(entry);
            entry.Active = true;
            HasReminders = ReminderEntries.Count > 0;
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

    public ReminderEntry? GetEntry(string kind, string entryId, string fromMod)
    {
        return ReminderEntries.FirstOrDefault(en => en.FromMod == fromMod && en.Kind == kind && en.EntryId == entryId);
    }

    internal void ReplaceAllBundleEntries()
    {
        for (int i = 0; i < ReminderEntries.Count; ++i)
        {
            ReminderEntry entry = ReminderEntries[i];
            if (ReminderEntries[i].Kind == ReminderEntryFactory.Kind_CommunityCenterBundle)
            {
                entry = new(entry.Kind, entry.EntryId, entry.FromMod);
                ReminderEntries[i] = entry;
                entry.Active = true;
            }
        }
    }
}
