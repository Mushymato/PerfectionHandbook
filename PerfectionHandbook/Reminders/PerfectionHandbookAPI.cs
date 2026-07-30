using StardewModdingAPI;

namespace PerfectionHandbook.Reminders;

public sealed class PerfectionHandbookAPI(IModInfo mod) : IPerfectionHandbookAPI
{
    private readonly string modId = mod.Manifest.UniqueID;

    /// <inheritdoc/>
    public void RegisterReminderKind(string kind, TryMakeReminderEntryDisplay factoryMethod)
    {
        ReminderEntryFactory.AddEntryMaker(modId, kind, factoryMethod);
    }

    /// <inheritdoc/>
    public IReminderEntry GetOrCreateReminder(string kind, string entry)
    {
        return MenuHandler.Reminders.GetOrCreateEntry(kind, entry, mod.Manifest.UniqueID);
    }

    /// <inheritdoc/>
    public void ToggleReminder(IReminderEntry entry)
    {
        if (entry is not ReminderEntry reminderEntry)
            throw new ArgumentException(
                $"{nameof(ToggleReminder)} arg 'entry' has unexpected type: '{entry.GetType().AssemblyQualifiedName}'"
            );
        MenuHandler.Reminders.ToggleEntry(reminderEntry);
    }

    /// <inheritdoc/>
    public void RemoveReminder(IReminderEntry entry)
    {
        if (entry is not ReminderEntry reminderEntry)
            throw new ArgumentException(
                $"{nameof(RemoveReminder)} arg 'entry' has unexpected type: '{entry.GetType().AssemblyQualifiedName}'"
            );
        MenuHandler.Reminders.RemoveEntry(reminderEntry);
    }
}
