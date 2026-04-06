using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.Locations;

namespace PerfectionHandbook.GUI;

public sealed record MuseumDonateDisplay(ItemInfo Info, ReprObject? OwnedRepr)
    : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed { get; protected set; } = !LibraryMuseum.HasDonatedArtifact(Info.Datum.ItemId);

    public override void SetStatus(Farmer who) { }
}

public sealed class GoalMuseumDonateContext(GoalContext goalCtx)
    : AbstractItemCountContext<MuseumDonateDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsMuseumDonation;

    protected override MuseumDonateDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr);
}
