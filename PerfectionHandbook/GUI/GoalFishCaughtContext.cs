using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record FishCaughtDisplay(ItemInfo Info, ReprObject? OwnedRepr) : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed => caughtCount == 0;
    public int caughtCount = 0;
    private int biggestCatch = 0;

    public override void SetStatus(Farmer who)
    {
        if (who.fishCaught.TryGetValue(Info.Datum.QualifiedItemId, out int[] pair))
        {
            caughtCount = pair[0];
            biggestCatch = pair[1];
        }
        else
        {
            caughtCount = 0;
            biggestCatch = 0;
        }
        OwnedRepr?.SetReprStack(caughtCount);
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public override string GetTooltipDesc()
    {
        if (OwnedRepr == null || caughtCount == 0)
        {
            return Info.Datum.Description;
        }
        return string.Concat(
            Info.Datum.Description,
            Environment.NewLine,
            Environment.NewLine,
            I18n.Ui_FishCatch(caughtCount, biggestCatch)
        );
    }
}

public sealed class GoalFishCaughtContext(GoalContext goalCtx) : AbstractItemCountContext<FishCaughtDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsCatchableFish;

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    protected override FishCaughtDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr);
}
