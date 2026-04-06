using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record ItemShippedDisplay(ItemInfo Info, ReprObject? OwnedRepr)
    : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override void SetStatus(Farmer who)
    {
        Needed = !who.basicShipped.ContainsKey(Info.Datum.ItemId);
    }
}

public sealed class GoalItemShippedContext(GoalContext goalCtx) : AbstractItemCountContext<ItemShippedDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsPotentialShipped;

    protected override ItemShippedDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr);
}
