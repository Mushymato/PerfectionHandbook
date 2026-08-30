using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;

namespace PerfectionHandbook.Models;

public static class AssetManager
{
    public const string AssetName_LocationStrings = $"{ModEntry.ModId}.LocationStrings";
    public const string AssetName_EventDesc = $"{ModEntry.ModId}/EventDesc";

    private static Dictionary<string, EventDescriptionData>? _eventDesc;
    public static Dictionary<string, EventDescriptionData> EventDesc =>
        _eventDesc ??= Game1.content.Load<Dictionary<string, EventDescriptionData>>(AssetName_EventDesc);

    public static void Setup()
    {
        ModEntry.help.Events.Content.AssetRequested += OnAssetRequested;
        ModEntry.help.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // add location names to Data/Locations
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
        {
            e.Edit(Edit_DataLocations, AssetEditPriority.Late + 100);
        }
        // location name strings from my i18n
        else if (e.Name.IsEquivalentTo(AssetName_LocationStrings))
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
        // mod provided event descriptions
        else if (e.NameWithoutLocale.IsEquivalentTo(AssetName_EventDesc))
        {
            e.LoadFrom(static () => new Dictionary<string, EventDescriptionData>(), AssetLoadPriority.Exclusive);
        }
    }

    private static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo(AssetName_EventDesc)))
        {
            _eventDesc = null;
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
