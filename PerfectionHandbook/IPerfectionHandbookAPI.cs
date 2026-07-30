using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerfectionHandbook;

/// <summary>
/// Information about a given reminder entry
///
/// </summary>
public interface IReminderEntry
{
    /// <summary>
    /// What kind of reminder this is,
    /// use to associate this entry with a <see cref="TryMakeReminderEntryDisplay"/> to make <see cref="IReminderEntryDisplay"/>
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// An id to identify your reminder,
    /// passed to a <see cref="TryMakeReminderEntryDisplay"/> to make <see cref="IReminderEntryDisplay"/>.
    /// </summary>
    string EntryId { get; }

    /// <summary>
    /// Whether this reminder active (i.e. player has added it to their reminder hard).
    /// This does not track whether the reminder UI is visible, only that this reminder is in the list.
    /// </summary>
    bool Active { get; }
}

/// <summary>Information neccesary to display the reminder</summary>
public interface IReminderEntryDisplay
{
    /// <summary>Text to display on the reminder, e.g. "Ship: Carrot"</summary>
    string Text { get; }

    /// <summary>Texture to use as icon, which will be drawn at 32x32</summary>
    Texture2D Texture { get; }

    /// <summary>Source rect of the texture</summary>
    Rectangle SourceRect { get; }

    /// <summary>Count to display (only when greater than 1)</summary>
    int Count { get; }

    /// <summary>Additional reminders displayed with a caret under the main reminder</summary>
    public IEnumerable<IReminderEntryDisplay>? SubReminders { get; }
}

/// <summary>
/// The factory method that Perfection Handbook will call to obtain display info before rendering the reminder.
/// This is expected to be associated with a specific reminder kind.
/// <seealso cref="IPerfectionHandbook.RegisterReminderEntryKind(string, TryMakeReminderEntryDisplay)"/>
/// </summary>
/// <param name="entryId">The entry id, specific to a particular kind</param>
/// <param name="entryDisplay">Entry display info</param>
/// <returns>
/// Whether the entryId is considered valid for display.
/// If this returns false, Perfection Handbook will remove the associated entry from the save file.
/// </returns>
public delegate bool TryMakeReminderEntryDisplay(
    string entryId,
    [NotNullWhen(true)] out IReminderEntryDisplay? entryDisplay
);

public interface IPerfectionHandbook
{
    /// <summary>
    /// Registers a new kind of reminders. This reminder kind is always scoped to your mod,
    /// i.e. 2 mods can both have reminder kind 'Goal' and they will be considered different.
    /// </summary>
    /// <param name="kind">kind name</param>
    /// <param name="factoryMethod">delegate used to create <see cref="IReminderEntryDisplay"/></param>
    /// <returns></returns>
    public void RegisterReminderEntryKind(string kind, TryMakeReminderEntryDisplay factoryMethod);

    /// <summary>
    /// Toggle a reminder entry.
    /// When entry is not in current reminders, this adds the entry.
    /// When entry is in current reminders, this removes the entry.
    /// Side Effect: another entry may be pushed out as a result of adding the new entry.
    /// </summary>
    /// <param name="kind">kind name</param>
    /// <param name="entryId">entry id</param>
    /// <returns>True if entry is added, false if entry is removed</returns>
    public bool ToggleReminder(IReminderEntry entry);
}
