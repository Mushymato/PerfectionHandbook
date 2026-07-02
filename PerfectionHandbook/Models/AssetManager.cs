using Force.DeepCloner;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;

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
#if DEBUG
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(
                asset =>
                {
                    ModEntry.Log("CHEEZ", LogLevel.Warn);
                    IDictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;
                    ObjectData cheese = data["424"];
                    for (int i = 0; i < 50000; i++)
                    {
                        ObjectData cheeseI = cheese.DeepClone();
                        cheeseI.Name = $"{ModEntry.ModId}_cheez_{i}";
                        cheeseI.DisplayName = $"{cheese.DisplayName}-{i}";
                        data[cheeseI.Name] = cheeseI;
                    }
                },
                AssetEditPriority.Late
            );
        }
#endif
    }
}
