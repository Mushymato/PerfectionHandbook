using System.ComponentModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using StardewModdingAPI.Utilities;

namespace PerfectionHandbook.Models;

public sealed class ModConfig
{
    public int ItemPerPage { get; set; } = 400;
    public KeybindList ShowHandbookKey { get; set; } = KeybindList.Parse("RightShift+H");
    public KeybindList RemindersToggleKey { get; set; } = KeybindList.Parse("LeftShift+H");
    public KeybindList RemindersEditModifierKey { get; set; } = KeybindList.Parse("LeftAlt");
    public int RemindersMaxCount { get; set; } = 12;
    public Vector2 RemindersHUDPosition { get; set; } = new(64, 64);
}

internal sealed class ModConfigContext(ModConfig config) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void WriteConfig()
    {
        ModEntry.help.WriteConfig(config);
    }

    public void RaisePropertyChanged(string propName)
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
}
