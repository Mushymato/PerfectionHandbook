using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public record ItemShippedDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    public override void SetStatus(Farmer who)
    {
        completedCount = who.basicShipped.GetValueOrDefault(Info.Datum.ItemId, 0);
        UpdateCount();
        OnPropertyChanged(new(nameof(Tooltip)));
    }
}

public sealed class GoalItemShippedContext(GoalContext goalCtx) : AbstractItemCountContext<ItemShippedDisplay>(goalCtx)
{
    public override string CompleteCountToggleText => I18n.Ui_CountingShipped();

    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsPotentialShipped;

    protected override ItemShippedDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);
}
