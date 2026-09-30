using System.Text;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.FruitTrees;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed record FruitTreeFruitDisplay(ItemInfo Info, SDUIEdges Margin);

public sealed partial record FruitTreeDisplay(
    string Id,
    ItemInfo SaplingInfo,
    FruitTreeData Data,
    int Count,
    string DisplayText,
    SDUISprite Sprite,
    IReadOnlyList<FruitTreeFruitDisplay> FruitDisplays
) : IPageDisplayEntry
{
    public bool Needed => true;

    public readonly Color DisplayTint = Count > 0 ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public bool SearchMatch(string txt)
    {
        if (DisplayText.ContainsIgnoreCase(txt))
            return true;
        return FruitDisplays.Any(disp => disp.Info.SearchMatch(txt));
    }

    public void SetStatus(Farmer who) { }

    public ReminderEntry? Reminder => throw new NotImplementedException();

    public static FruitTreeDisplay Make(
        string treeId,
        ItemInfo saplingInfo,
        FruitTreeData fruitTreeData,
        Dictionary<string, Dictionary<string, int>> treeCounts
    )
    {
        Rectangle sourceRect = new(
            (12 + (int)(fruitTreeData.Seasons.Any() ? fruitTreeData.Seasons[0] : Game1.season) * 3) * 16,
            fruitTreeData.TextureSpriteRow * 5 * 16,
            48,
            80
        );
        SDUISprite sprite = new(DrawHelper.SafeLoad(fruitTreeData.Texture ?? "TileSheets\\fruitTrees"), sourceRect);

        StringBuilder sb = HandbookContext.sb;
        sb.AppendLine(TokenParser.ParseText(fruitTreeData.DisplayName) ?? treeId);

        int plantedCount = 0;
        if (treeCounts.TryGetValue(treeId, out Dictionary<string, int>? locations))
        {
            plantedCount = locations.Values.Sum();
            sb.Append(I18n.Ui_PlantedCount(plantedCount));
            foreach ((string loc, int count) in locations)
            {
                sb.Append('\n');
                sb.Append(I18n.Ui_PlantedLocationCount(loc, count));
            }
        }
        else
        {
            sb.Append(I18n.Ui_PlantedCount(plantedCount));
        }

        string displayText = sb.ToString();
        sb.Clear();

        List<FruitTreeFruitDisplay> fruitDisplays = [];
        if (plantedCount > 0 && (fruitTreeData.Fruit?.Any() ?? false))
        {
            const int MAX_COUNT = 3;

            foreach (FruitTreeFruitData fruit in fruitTreeData.Fruit)
            {
                if (
                    ItemRegistry.QualifyItemId(fruit.ItemId) is string qId
                    && ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? info)
                )
                {
                    fruitDisplays.Add(new(info, GetFruitOffset(fruitDisplays)));
                }
                if (fruitDisplays.Count >= MAX_COUNT)
                    break;
            }
            while (fruitDisplays.Count < MAX_COUNT)
            {
                fruitDisplays.Add(new(fruitDisplays[^1].Info, GetFruitOffset(fruitDisplays)));
            }
        }
        return new(treeId, saplingInfo, fruitTreeData, plantedCount, displayText, sprite, fruitDisplays);

        static SDUIEdges GetFruitOffset(List<FruitTreeFruitDisplay> fruitDisplays)
        {
            const int OFFSET = 12;
            return fruitDisplays.Count switch
            {
                0 => new(OFFSET, OFFSET + 36, 0, 0),
                1 => new(OFFSET + 72, OFFSET, 0, 0),
                _ => new(OFFSET + 48, OFFSET + 72, 0, 0),
            };
        }
    }
}

public sealed partial class MiscFruitTreeContext(IGoalContext goalCtx)
    : AbstractPageListContext<FruitTreeDisplay>(
        goalCtx,
        canToggleNeeded: false,
        canToggleCountMode: false,
        canSetReminders: false,
        itemPerPageModifier: 3.3 / 13.0
    )
{
    public override bool HasSortModes => true;
    protected override List<PageSortMode> ValidSortModes => [PageSortMode.Count, PageSortMode.Name];

    protected override IReadOnlyList<FruitTreeDisplay> MakeAllDisplay()
    {
        Dictionary<string, Dictionary<string, int>> treeToLocations = [];
        Utility.ForEachLocation(location =>
        {
            if (!location.IsFarm && !(location.GetParentLocation()?.IsFarm ?? false))
                return true;
            foreach (TerrainFeature feature in location.terrainFeatures.Values)
            {
                if (feature is not FruitTree fruitTree)
                    continue;
                string treeId = fruitTree.treeId.Value;
                if (!string.IsNullOrEmpty(treeId) && fruitTree.texture != null)
                {
                    if (!treeToLocations.TryGetValue(treeId, out Dictionary<string, int>? locations))
                    {
                        locations = [];
                        treeToLocations[treeId] = locations;
                    }
                    locations[location.DisplayName] = 1 + locations.GetValueOrDefault(location.DisplayName, 0);
                }
            }
            return true;
        });

        List<FruitTreeDisplay> fruitTreeDisplays = [];
        foreach ((string treeId, FruitTreeData fruitTreeData) in Game1.fruitTreeData)
        {
            if (
                ItemRegistry.QualifyItemId(treeId) is string qId
                && ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? info)
            )
            {
                fruitTreeDisplays.Add(FruitTreeDisplay.Make(treeId, info, fruitTreeData, treeToLocations));
            }
        }
        return fruitTreeDisplays;
    }

    protected override List<FruitTreeDisplay> SortAllDisplay(List<FruitTreeDisplay> displayList)
    {
        return SortMode switch
        {
            PageSortMode.Name => displayList
                .OrderBy(static disp => disp.SaplingInfo.DisplayName, ModEntry.displayStringComparer)
                .ToList(),
            PageSortMode.Count => displayList.OrderByDescending(static disp => disp.Count).ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }
}
