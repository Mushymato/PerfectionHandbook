using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record ItemShippedDisplay(ItemInfo Info, Item? OwnedRepr) : IPageDisplayEntry
{
    public Color DisplayTint = OwnedRepr != null ? Color.White : Color.DimGray * 0.4f;
    public bool Needed { get; private set; } = false;
    public Item ReprItem = OwnedRepr ?? Info.ReprItem;

    public readonly SDUITooltipData Tooltip = new(
        OwnedRepr == null
            ? Info.Datum.Description
            : string.Concat(
                Info.Datum.Description,
                Environment.NewLine,
                Environment.NewLine,
                I18n.Ui_OwnedCount(OwnedRepr.Stack)
            ),
        Info.Datum.DisplayName,
        Info.ReprItem
    );

    public void SetStatus(Farmer who)
    {
        Needed = !who.basicShipped.ContainsKey(Info.Datum.ItemId);
    }
}

public sealed class GoalItemShippedContext(GoalContext GoalCtx)
    : AbstractGoalPageListContext<ItemShippedDisplay>(GoalCtx)
{
    protected override IReadOnlyList<ItemShippedDisplay> MakeAllDisplay()
    {
        List<ItemShippedDisplay> shippingList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            if (!itemInfo.IsPotentialShipped)
                continue;
            Item? ownedRepr = null;
            if (GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(itemInfo.Datum.QualifiedItemId, out OwnedItemGroup? group))
            {
                ownedRepr = group.CountRepr;
            }
            shippingList.Add(new(itemInfo, ownedRepr));
        }
        return shippingList;
    }
}
