using PerfectionHandbook.Integration;
using StardewModdingAPI.Utilities;

namespace PerfectionHandbook.Models;

public sealed class ModConfig
{
    public int RowPerPage { get; set; } = 16;
    public KeybindList ShowHandbookKey { get; set; } = KeybindList.Parse("RightShift+H");
    public KeybindList RemindersToggleKey { get; set; } = KeybindList.Parse("LeftShift+H");
    public KeybindList RemindersEditModifierKey { get; set; } = KeybindList.Parse("LeftAlt");
    public int RemindersMaxCount { get; set; } = 12;
    public bool RemindersDefaultExpanded { get; set; } = true;
    public SDUINineGridPlacement RemindersHUDPosition
    {
        get => field;
        set
        {
            field = value;
            foreach ((_, RemindersHUD reminderHud) in MenuHandler.reminders.GetActiveValues())
            {
                reminderHud.Reposition();
            }
        }
    } = new(SDUIAlignment.Start, SDUIAlignment.Start, new(64, 64));
}
