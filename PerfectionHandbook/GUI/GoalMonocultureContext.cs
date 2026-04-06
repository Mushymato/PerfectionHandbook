using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record ShippedCountDisplay(ItemInfo Info, ReprObject? OwnedRepr, int Count)
    : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed => shippedCount < Count;
    private int shippedCount = 0;

    public override void SetStatus(Farmer who)
    {
        shippedCount = who.basicShipped.GetValueOrDefault(Info.Datum.ItemId, 0);
        Needed = shippedCount < Count;
        OwnedRepr?.SetReprStack(shippedCount);
        DisplayTint = shippedCount <= 0 ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
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
            I18n.Ui_ShippedCount(OwnedRepr.Stack)
        );
    }
}

public sealed class GoalMonocultureContext(GoalContext goalCtx) : AbstractItemCountContext<ShippedCountDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.CountForMonoculture;

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    protected override ShippedCountDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr, 300);
}
