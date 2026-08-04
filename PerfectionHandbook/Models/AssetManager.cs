using Force.DeepCloner;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;

namespace PerfectionHandbook.Models;

public static class AssetManager
{
    public const string AssetName_LocationStrings = $"{ModEntry.ModId}.LocationStrings";
    public const string AssetName_EventInfo = $"{ModEntry.ModId}/EventInfo";

    public static void Setup()
    {
        ModEntry.help.Events.Content.AssetRequested += OnAssetRequested;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // location names (derive from world map)
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
        {
            e.Edit(Edit_DataLocations, AssetEditPriority.Late);
        }

        // location names from my i18n
        if (e.Name.IsEquivalentTo(AssetName_LocationStrings))
        {
            string stringsAsset = Path.Combine(
                "i18n",
                e.Name.LanguageCode.ToString() ?? "default",
                "location_names.json"
            );
            if (File.Exists(Path.Combine(ModEntry.help.DirectoryPath, stringsAsset)))
            {
                e.LoadFromModFile<Dictionary<string, string>>(stringsAsset, AssetLoadPriority.Exclusive);
            }
            else
            {
                e.LoadFromModFile<Dictionary<string, string>>(
                    "i18n/default/location_names.json",
                    AssetLoadPriority.Exclusive
                );
            }
        }

        // event info
        if (e.NameWithoutLocale.IsEquivalentTo(AssetName_EventInfo))
        {
            e.LoadFrom(() => new Dictionary<string, EventDescriptionData>(), AssetLoadPriority.Exclusive);
            return;
        }
    }

    private static void Edit_DataLocations(IAssetData asset)
    {
        Dictionary<string, string> locationNames = Game1.content.Load<Dictionary<string, string>>(
            AssetName_LocationStrings
        );
        IDictionary<string, LocationData> locationData = asset.AsDictionary<string, LocationData>().Data;
        foreach ((string key, LocationData data) in locationData)
        {
            string locKey = $"location.{key}";
            if (data.DisplayName == null && locationNames.TryGetValue(locKey, out string? locName))
            {
                data.DisplayName = locName;
            }
        }
    }
}
