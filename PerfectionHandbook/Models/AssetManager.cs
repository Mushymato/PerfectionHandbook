using StardewModdingAPI.Events;

namespace PerfectionHandbook.Models;

public static class AssetManager
{
    public const string AssetName_EventInfo = $"{ModEntry.ModId}/EventInfo";

    public static void Register()
    {
        ModEntry.help.Events.Content.AssetRequested += OnAssetRequested;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // event info
        if (e.NameWithoutLocale.IsEquivalentTo(AssetName_EventInfo))
        {
            e.LoadFrom(() => new Dictionary<string, EventDescriptionData>(), AssetLoadPriority.Exclusive);
            return;
        }
    }
}
