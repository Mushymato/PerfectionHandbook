using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record ShippedCountDisplay(ItemInfo Info, ReprObject? OwnedRepr, int RequiredCount)
    : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed => Count < RequiredCount;

    public override void SetStatus(Farmer who)
    {
        Count = who.basicShipped.GetValueOrDefault(Info.Datum.ItemId, 0);
        OwnedRepr?.SetReprStack(Count);
        DisplayTint = Count <= 0 ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public override string GetTooltipDesc()
    {
        if (OwnedRepr == null)
            return Info.Datum.Description;
        return string.Concat(
            Info.Datum.Description,
            Environment.NewLine,
            Environment.NewLine,
            I18n.Ui_ShippedCount(OwnedRepr.ReprStack)
        );
    }
}

public sealed class GoalMonocultureContext(GoalContext goalCtx) : AbstractItemCountContext<ShippedCountDisplay>(goalCtx)
{
    public override bool ShowDetail => true;

    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.CountForMonoculture;

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    protected override ShippedCountDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr, 300);
}
