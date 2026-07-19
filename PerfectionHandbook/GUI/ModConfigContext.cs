using System.ComponentModel;
using PerfectionHandbook;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewModdingAPI.Utilities;

internal sealed class ModConfigContext(ModConfig config) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void WriteConfig()
    {
        ModEntry.help.WriteConfig(config);
    }

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
        WriteConfig();
    }

    public IntSpinBoxViewModel ItemPerPageSpinBox =>
        new(
            () => config.ItemPerPage,
            (value) =>
            {
                config.ItemPerPage = value;
                WriteConfig();
            },
            200,
            int.MaxValue,
            100
        );

    public KeybindList ShowHandbookKey
    {
        get => config.ShowHandbookKey;
        set
        {
            if (value != config.ShowHandbookKey)
            {
                config.ShowHandbookKey = value;
                RaisePropertyChanged(nameof(ShowHandbookKey));
            }
        }
    }

    public KeybindList RemindersToggleKey
    {
        get => config.RemindersToggleKey;
        set
        {
            if (value != config.RemindersToggleKey)
            {
                config.RemindersToggleKey = value;
                RaisePropertyChanged(nameof(RemindersToggleKey));
            }
        }
    }

    public KeybindList RemindersEditModifierKey
    {
        get => config.RemindersEditModifierKey;
        set
        {
            if (value != config.RemindersEditModifierKey)
            {
                config.RemindersEditModifierKey = value;
                RaisePropertyChanged(nameof(RemindersEditModifierKey));
            }
        }
    }
    public IntSpinBoxViewModel RemindersMaxCountSpinBox =>
        new(
            () => config.RemindersMaxCount,
            (value) =>
            {
                config.RemindersMaxCount = value;
                WriteConfig();
            },
            1,
            int.MaxValue,
            1
        );

    public SDUINineGridPlacement RemindersHUDPosition
    {
        get => config.RemindersHUDPosition;
        set
        {
            if (config.RemindersHUDPosition != value)
            {
                config.RemindersHUDPosition = value;
                RaisePropertyChanged(nameof(RemindersHUDPosition));
            }
        }
    }

    public RemindersContext RemindersHUDCtx => MenuHandler.reminders.Value.ctx;
}
