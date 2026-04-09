using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;

namespace PerfectionHandbook.GUI;

public sealed class GoalPolycultureContext(GoalContext goalCtx) : AbstractItemCountContext<ShippedCountDisplay>(goalCtx)
{
    public override bool ShowDetail => true;

    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.CountForPolyculture;

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    protected override ShippedCountDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr, 15);
}
