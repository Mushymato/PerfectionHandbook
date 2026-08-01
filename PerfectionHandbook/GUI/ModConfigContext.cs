using System.ComponentModel;
using PerfectionHandbook;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewModdingAPI.Utilities;

internal sealed class ModConfigContext(ModConfig config) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
        ModEntry.help.WriteConfig(config);
    }

    public IntSpinBoxViewModel ItemPerPageSpinBox = new(
        () => config.ItemPerPage,
        (value) =>
        {
            config.ItemPerPage = value;
            ModEntry.help.WriteConfig(config);
        },
        50,
        int.MaxValue,
        50
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
    public IntSpinBoxViewModel RemindersMaxCountSpinBox = new(
        () => config.RemindersMaxCount,
        (value) =>
        {
            config.RemindersMaxCount = value;
            ModEntry.help.WriteConfig(config);
        },
        1,
        int.MaxValue,
        1
    );

    public bool RemindersDefaultExpanded
    {
        get => config.RemindersDefaultExpanded;
        set
        {
            if (config.RemindersDefaultExpanded != value)
            {
                config.RemindersDefaultExpanded = value;
                RaisePropertyChanged(nameof(RemindersDefaultExpanded));
            }
        }
    }

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

    public RemindersContext RemindersHUDCtx => MenuHandler.Reminders.ctx;

    public void Reset()
    {
        ModConfig defaultConfig = new();

        ItemPerPageSpinBox.Value = defaultConfig.ItemPerPage;
        RemindersMaxCountSpinBox.Value = defaultConfig.RemindersMaxCount;

        ShowHandbookKey = defaultConfig.ShowHandbookKey;
        RemindersToggleKey = defaultConfig.RemindersToggleKey;
        RemindersEditModifierKey = defaultConfig.RemindersEditModifierKey;
        RemindersHUDPosition = defaultConfig.RemindersHUDPosition;
    }
}
