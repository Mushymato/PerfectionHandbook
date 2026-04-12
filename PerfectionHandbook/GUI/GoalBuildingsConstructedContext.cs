using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed record BuildingsBuiltDisplay(
    string BuildingId,
    int BuiltCount,
    string BuildingName,
    SDUISprite BuildingSprite
) : IPageDisplayEntry
{
    public Color DisplayTint = BuiltCount == 0 ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
    public bool Needed => BuiltCount == 0;

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return BuildingName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who) { }
}

public sealed class GoalBuildingsConstructedContext(GoalContext goalCtx)
    : AbstractPageListContext<BuildingsBuiltDisplay>(goalCtx)
{
    private static readonly string[] ObelisksAndClock =
    [
        "Water Obelisk",
        "Earth Obelisk",
        "Desert Obelisk",
        "Island Obelisk",
        "Gold Clock",
    ];

    protected override IReadOnlyList<BuildingsBuiltDisplay> MakeAllDisplay()
    {
        List<BuildingsBuiltDisplay> builtDisplay = [];
        foreach (string buildingId in ObelisksAndClock)
        {
            if (
                !Game1.buildingData.TryGetValue(buildingId, out BuildingData? buildingData)
                || !Game1.content.DoesAssetExist<Texture2D>(buildingData.Texture)
            )
            {
                continue;
            }
            int builtCount = Game1.GetNumberBuildingsConstructed(buildingId);
            Texture2D buildingTx = Game1.content.Load<Texture2D>(buildingData.Texture);
            builtDisplay.Add(
                new(
                    buildingId,
                    builtCount,
                    TokenParser.ParseText(buildingData.Name) ?? buildingId,
                    new(buildingTx, buildingData.SourceRect.IsEmpty ? buildingTx.Bounds : buildingData.SourceRect)
                )
            );
        }
        return builtDisplay;
    }
}
