using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.Locations;

namespace PerfectionHandbook.GUI;

public sealed record MuseumDonateDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    private readonly bool needed = !LibraryMuseum.HasDonatedArtifact(Info.Datum.ItemId);
    public override bool Needed => needed;

    public override void SetStatus(Farmer who) { }
}

public sealed class GoalMuseumDonateContext(GoalContext goalCtx)
    : AbstractItemCountContext<MuseumDonateDisplay>(goalCtx, false)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsMuseumDonation;

    protected override MuseumDonateDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);
}
