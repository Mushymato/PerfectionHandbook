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
    int PlantedCount,
    string DisplayText,
    SDUISprite Sprite,
    IReadOnlyList<FruitTreeFruitDisplay> FruitDisplays
) : IPageDisplayEntry
{
    public bool Needed => true;

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
        Dictionary<string, (int, int)> treeCount
    )
    {
        int plantedCount = 0;
        int maxStage = 0;
        if (treeCount.TryGetValue(treeId, out (int, int) counters))
        {
            plantedCount = counters.Item1;
            maxStage = counters.Item1;
        }
        int spriteRowNumber = fruitTreeData.TextureSpriteRow;
        Rectangle sourceRect = maxStage switch
        {
            0 => new Rectangle(0, spriteRowNumber * 5 * 16, 48, 80),
            1 => new Rectangle(48, spriteRowNumber * 5 * 16, 48, 80),
            2 => new Rectangle(96, spriteRowNumber * 5 * 16, 48, 80),
            3 => new Rectangle(144, spriteRowNumber * 5 * 16, 48, 80),
            _ => new Rectangle((12 + (int)Game1.season * 3) * 16, spriteRowNumber * 5 * 16, 48, 80),
        };
        SDUISprite sprite = new(DrawHelper.SafeLoad(fruitTreeData.Texture ?? "TileSheets\\fruitTrees"), sourceRect);
        StringBuilder sb = HandbookContext.sb;
        sb.AppendLine(TokenParser.ParseText(fruitTreeData.DisplayName) ?? treeId);
        sb.Append(I18n.Ui_PlantedCount(plantedCount));
        string displayText = sb.ToString();
        sb.Clear();
        List<FruitTreeFruitDisplay> fruitDisplays = [];
        if (plantedCount > 0 && maxStage >= 4 && (fruitTreeData.Fruit?.Any() ?? false))
        {
            const int MAX_COUNT = 3;
            const int OFFSET = 16;
            foreach (FruitTreeFruitData fruit in fruitTreeData.Fruit)
            {
                if (
                    ItemRegistry.QualifyItemId(fruit.ItemId) is string qId
                    && ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? info)
                )
                {
                    fruitDisplays.Add(
                        new(
                            info,
                            fruitDisplays.Count switch
                            {
                                0 => new(OFFSET, OFFSET + 32, 0, 0),
                                1 => new(OFFSET + 96, OFFSET, 0, 0),
                                _ => new(OFFSET + 64, OFFSET + 64, 0, 0),
                            }
                        )
                    );
                }
                if (fruitDisplays.Count >= MAX_COUNT)
                    break;
            }
            while (fruitDisplays.Count < MAX_COUNT)
            {
                fruitDisplays.Add(
                    new(
                        fruitDisplays[0].Info,
                        fruitDisplays.Count switch
                        {
                            0 => new(OFFSET, OFFSET + 48, 0, 0),
                            1 => new(OFFSET + 96, OFFSET, 0, 0),
                            _ => new(OFFSET + 64, OFFSET + 96, 0, 0),
                        }
                    )
                );
            }
            ModEntry.Log($"fruitDisplays: {fruitDisplays.Count}");
        }
        return new(treeId, saplingInfo, fruitTreeData, plantedCount, displayText, sprite, fruitDisplays);
    }
}

public sealed partial class MiscFruitTreeContext(IGoalContext goalCtx)
    : AbstractPageListContext<FruitTreeDisplay>(
        goalCtx,
        canToggleNeeded: false,
        canToggleCountMode: false,
        canSetReminders: false
    )
{
    protected override IReadOnlyList<FruitTreeDisplay> MakeAllDisplay()
    {
        Dictionary<string, (int, int)> treeCount = [];
        Utility.ForEachLocation(location =>
        {
            foreach (TerrainFeature feature in location.terrainFeatures.Values)
            {
                if (!location.IsFarm)
                    continue;
                if (feature is not FruitTree fruitTree)
                    continue;
                string treeId = fruitTree.treeId.Value;
                if (!string.IsNullOrEmpty(treeId) && fruitTree.texture != null)
                {
                    if (treeCount.TryGetValue(treeId, out (int, int) count))
                    {
                        treeCount[treeId] = new(count.Item1 + 1, Math.Max(count.Item2, fruitTree.growthStage.Value));
                    }
                    else
                    {
                        treeCount[treeId] = new(1, fruitTree.growthStage.Value);
                    }
                }
            }
            return false;
        });

        List<FruitTreeDisplay> fruitTreeDisplays = [];
        foreach ((string treeId, FruitTreeData fruitTreeData) in Game1.fruitTreeData)
        {
            if (
                ItemRegistry.QualifyItemId(treeId) is string qId
                && ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? info)
            )
            {
                fruitTreeDisplays.Add(FruitTreeDisplay.Make(treeId, info, fruitTreeData, treeCount));
            }
        }
        return fruitTreeDisplays;
    }
}
