using System.ComponentModel;
using PerfectionHandbook.GUI.Shared;
using StardewModdingAPI.Utilities;

namespace PerfectionHandbook.Models;

public class ModConfig
{
    public virtual int ItemPerPage { get; set; } = 800;
    public virtual KeybindList ShowHandbookKey { get; set; } = KeybindList.Parse("LeftShift+H");
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
}
