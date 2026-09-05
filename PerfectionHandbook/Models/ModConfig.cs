using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace PerfectionHandbook.Models;

public sealed class ModConfig
{
    public int RowPerPage { get; set; } = 16;
    public KeybindList ShowHandbookKey { get; set; } = KeybindList.Parse("RightShift+H");
    public int AutoExportCardPeriod { get; set; } = 7;
    public Point CardDimension { get; set; } = new(1440, 620);
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

    public void Register(IManifest mod)
    {
        if (
            ModEntry.help.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu")
            is not IGenericModConfigMenuApi gmcm
        )
        {
            return;
        }
        gmcm.Register(
            mod,
            reset: () =>
            {
                ShowHandbookKey = KeybindList.Parse("RightShift+H");
                RemindersToggleKey = KeybindList.Parse("LeftShift+H");
                RemindersEditModifierKey = KeybindList.Parse("LeftAlt");
                ModEntry.help.WriteConfig(this);
            },
            save: () =>
            {
                ModEntry.help.WriteConfig(this);
            }
        );
        gmcm.AddKeybindList(
            mod,
            () => ShowHandbookKey,
            (value) => ShowHandbookKey = value,
            I18n.Config_Name_ShowHandbookKey,
            I18n.Config_Desc_ShowHandbookKey
        );
        gmcm.AddKeybindList(
            mod,
            () => RemindersToggleKey,
            (value) => RemindersToggleKey = value,
            I18n.Config_Name_RemindersToggleKey,
            I18n.Config_Desc_RemindersToggleKey
        );
        gmcm.AddKeybindList(
            mod,
            () => RemindersEditModifierKey,
            (value) => RemindersEditModifierKey = value,
            I18n.Config_Name_RemindersEditModifierKey,
            I18n.Config_Desc_RemindersEditModifierKey
        );
        gmcm.AddParagraph(mod, I18n.Config_Gmcm_MoreConfigs);
    }
}
