using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley.Locations;

namespace PerfectionHandbook.GUI;

public sealed record MuseumDonateDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    private readonly bool needed = !LibraryMuseum.HasDonatedArtifact(Info.Datum.ItemId);
    public override bool Needed => needed;

    public override string ReminderKind => RemindersHUD.DonateKind;
    public override ReminderEntry? Reminder =>
        field ??= new(
            ReminderKind,
            Info.ReprItem.ItemId,
            Info.Sprite,
            I18n.Reminder_Verb_Donate(Info.Datum.DisplayName),
            1
        );
}

public sealed class GoalMuseumDonateContext(IGoalContext goalCtx)
    : AbstractItemCountContext<MuseumDonateDisplay>(goalCtx, canToggleNeeded: true, canToggleCountMode: false)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsMuseumDonation;

    protected override MuseumDonateDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);
}
