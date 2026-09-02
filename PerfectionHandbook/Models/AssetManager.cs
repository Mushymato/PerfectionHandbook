using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;

namespace PerfectionHandbook.Models;

public static class AssetManager
{
    public const string AssetName_LocationStrings = $"{ModEntry.ModId}.LocationStrings";
    public const string AssetName_EventDescStrings = $"{ModEntry.ModId}.EventDescStrings";
    public const string AssetName_EventDesc = $"{ModEntry.ModId}/EventDesc";
    public const string ObjectId_Book = $"{ModEntry.ModId}_book";
    public const string ObjectQId_Book = $"(O){ObjectId_Book}";

    private static Dictionary<string, EventDescriptionData>? _eventDesc;
    public static Dictionary<string, EventDescriptionData> EventDesc =>
        _eventDesc ??= Game1.content.Load<Dictionary<string, EventDescriptionData>>(AssetName_EventDesc);

    public static EventDescriptionData? GetEventDesc(string eventId)
    {
        if (EventDesc.TryGetValue(eventId, out EventDescriptionData? desc))
            return desc;
        return null;
    }

    internal static void OnAssetRequested(AssetRequestedEventArgs e)
    {
        // add location names to Data/Locations
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
        {
            e.Edit(Edit_DataLocations, AssetEditPriority.Late + 100);
        }
        // location name strings from my i18n
        else if (e.Name.IsEquivalentTo(AssetName_LocationStrings))
        {
            Load_StringsAsset(e, "location_names.json");
        }
        // event strings from my i18n
        else if (e.Name.IsEquivalentTo(AssetName_EventDescStrings))
        {
            Load_StringsAsset(e, "event_descriptions.json");
        }
        // mod provided event descriptions
        else if (e.NameWithoutLocale.IsEquivalentTo(AssetName_EventDesc))
        {
            e.LoadFromModFile<Dictionary<string, EventDescriptionData>>(
                "assets/eventdesc.json",
                AssetLoadPriority.Exclusive
            );
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(Edit_Objects, AssetEditPriority.Default);
        }
    }

    private static void Edit_Objects(IAssetData assets)
    {
        IDictionary<string, ObjectData> data = assets.AsDictionary<string, ObjectData>().Data;
        data[ObjectId_Book] = new()
        {
            Name = ObjectId_Book,
            DisplayName = I18n.Ui_Mod_Name(),
            Description = I18n.Ui_Mod_Desc(),
            Type = "Basic",
            Category = 0,
            Price = 2,
            Texture = "TileSheets\\Objects_2",
            SpriteIndex = 96,
            ExcludeFromFishingCollection = true,
            ExcludeFromShippingCollection = true,
            ContextTags = ["color_iridium", "book_item"],
        };
    }

    private static void Load_StringsAsset(AssetRequestedEventArgs e, string fileName)
    {
        string stringsAsset = Path.Combine("i18n", e.Name.LanguageCode.ToString() ?? "default", fileName);
        if (File.Exists(Path.Combine(ModEntry.help.DirectoryPath, stringsAsset)))
        {
            e.LoadFromModFile<Dictionary<string, string>>(stringsAsset, AssetLoadPriority.Exclusive);
        }
        else
        {
            e.LoadFromModFile<Dictionary<string, string>>(
                Path.Combine("i18n", "default", fileName),
                AssetLoadPriority.Exclusive
            );
        }
    }

    internal static void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
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
