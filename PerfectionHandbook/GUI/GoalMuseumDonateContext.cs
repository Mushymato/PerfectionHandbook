using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley.Locations;

namespace PerfectionHandbook.GUI;

public sealed record MuseumDonateDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    private readonly bool needed = !LibraryMuseum.HasDonatedArtifact(Info.Datum.ItemId);
    public override bool Needed => needed;

    public override ReminderEntry? Reminder { get; } =
        new(ReminderEntryFactory.Kind_MuseumDonate, Info.ReprItem.ItemId);
}

public sealed class GoalMuseumDonateContext(IGoalContext goalCtx)
    : AbstractItemCountContext<MuseumDonateDisplay>(goalCtx, canToggleNeeded: true, canToggleCountMode: false)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsMuseumDonation;

    protected override MuseumDonateDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);
}
