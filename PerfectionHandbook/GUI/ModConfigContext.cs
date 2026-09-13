using System.ComponentModel;
using PerfectionHandbook;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewModdingAPI.Utilities;

internal sealed class ModConfigContext(ModConfig config) : INotifyPropertyChanged, IPageContext
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
        ModEntry.help.WriteConfig(config);
    }

    public IntSpinBoxViewModel RowPerPageSpinBox = new(
        () => config.RowPerPage,
        (value) =>
        {
            if (config.RowPerPage != value)
            {
                config.RowPerPage = value;
                ModEntry.help.WriteConfig(config);
                return true;
            }
            return false;
        },
        2,
        int.MaxValue,
        1
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

    public IntSpinBoxViewModel AutoExportPeriodSpinBox = new(
        () => config.AutoExportCardPeriod,
        (value) =>
        {
            if (config.AutoExportCardPeriod != value)
            {
                config.AutoExportCardPeriod = value;
                ModEntry.help.WriteConfig(config);
                return true;
            }
            return false;
        },
        0,
        112,
        7
    );

    public IntSpinBoxViewModel CardWidthSpinBox = new(
        () => config.CardWidth,
        (value) =>
        {
            if (config.CardWidth != value)
            {
                config.CardWidth = value;
                ModEntry.help.WriteConfig(config);
                return true;
            }
            return false;
        },
        ModConfig.DEFAULT_CARD_X,
        int.MaxValue,
        20
    );

    public bool StrictItemOwnedCheck
    {
        get => config.StrictItemOwnedCheck;
        set
        {
            if (value != config.StrictItemOwnedCheck)
            {
                config.StrictItemOwnedCheck = value;
                RaisePropertyChanged(nameof(StrictItemOwnedCheck));
            }
        }
    }

    public bool HasModNamesAPI => ModEntry.modNameAPI != null;

    public bool EnableModNames
    {
        get => config.EnableModNames;
        set
        {
            if (value != config.EnableModNames)
            {
                config.EnableModNames = value;
                RaisePropertyChanged(nameof(EnableModNames));
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
            if (config.RemindersMaxCount != value)
            {
                config.RemindersMaxCount = value;
                ModEntry.help.WriteConfig(config);
                return true;
            }
            return false;
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

    public void ResetConfigsToDefault()
    {
        ModConfig defaultConfig = new();

        RowPerPageSpinBox.Value = defaultConfig.RowPerPage;
        RemindersMaxCountSpinBox.Value = defaultConfig.RemindersMaxCount;
        AutoExportPeriodSpinBox.Value = defaultConfig.AutoExportCardPeriod;
        CardWidthSpinBox.Value = defaultConfig.CardWidth;

        ShowHandbookKey = defaultConfig.ShowHandbookKey;
        StrictItemOwnedCheck = defaultConfig.StrictItemOwnedCheck;
        EnableModNames = defaultConfig.EnableModNames;
        RemindersToggleKey = defaultConfig.RemindersToggleKey;
        RemindersEditModifierKey = defaultConfig.RemindersEditModifierKey;
        RemindersHUDPosition = defaultConfig.RemindersHUDPosition;
        RemindersDefaultExpanded = defaultConfig.RemindersDefaultExpanded;
    }

    public bool TryOpenPage() => true;

    public bool TryExitPage() => true;
}
