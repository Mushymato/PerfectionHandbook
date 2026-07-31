using PerfectionHandbook.Integration;
using StardewModdingAPI.Utilities;

namespace PerfectionHandbook.Models;

public sealed class ModConfig
{
    public int ItemPerPage { get; set; } = 400;
    public KeybindList ShowHandbookKey { get; set; } = KeybindList.Parse("RightShift+H");
    public KeybindList RemindersToggleKey { get; set; } = KeybindList.Parse("LeftShift+H");
    public KeybindList RemindersEditModifierKey { get; set; } = KeybindList.Parse("LeftAlt");
    public int RemindersMaxCount { get; set; } = 12;

    // public SDUINineGridPlacement RemindersHUDPosition { get; set; } =
    //     new(SDUIAlignment.Start, SDUIAlignment.Start, new(64, 64));
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
