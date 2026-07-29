using System.Text;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.GameData.Locations;

namespace PerfectionHandbook.GUI;

public sealed record FishCaughtDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    public override bool Needed => Count == 0;
    private int biggestCatch = 0;
    internal IReadOnlyList<string>? canCatchIn = null;
    public override Color DisplayTint =>
        canCatchIn != null ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public override void SetStatus(Farmer who)
    {
        if (who.fishCaught.TryGetValue(Info.Datum.QualifiedItemId, out int[] pair))
        {
            completedCount = pair[0];
            biggestCatch = pair[1];
        }
        else
        {
            completedCount = 0;
            biggestCatch = 0;
        }
        Count = completedCount;
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public void SetCanCatchIn(IReadOnlyList<string> canCatchIn)
    {
        this.canCatchIn = canCatchIn.Any() ? canCatchIn : null;
    }

    private static readonly StringBuilder sb = new();

    public override string GetTooltipDesc()
    {
        sb.Append(Info.Datum.Description);
        if (Count > 0)
        {
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(
                I18n.Ui_FishCatch(Count, biggestCatch > 0 ? I18n.Ui_FishCatchLength(biggestCatch) : string.Empty)
            );
        }
        if (canCatchIn != null)
        {
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(I18n.Ui_FishFrom());
            sb.Append(Environment.NewLine);
            sb.Append("  ");
            sb.AppendJoin(Environment.NewLine + "  ", canCatchIn);
        }
        string result = sb.ToString();
        sb.Clear();
        return result;
    }

    public override string ReminderKind => RemindersHUD.FishingKind;
    public override ReminderEntry? Reminder =>
        field ??= new(
            ReminderKind,
            Info.ReprItem.QualifiedItemId,
            Info.Sprite,
            I18n.Reminder_Verb_Fish(Info.Datum.DisplayName),
            1
        );
}

public sealed class GoalFishCaughtContext(IGoalContext goalCtx)
    : AbstractItemCountContext<FishCaughtDisplay>(
        goalCtx,
        canToggleNeeded: true,
        canToggleCountMode: true,
        defaultCountMode: CountMode.Completed
    )
{
    public override string CompleteCountToggleText => I18n.Ui_CountingFished();

    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsCatchableFish;

    protected override FishCaughtDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);

    protected override List<FishCaughtDisplay> FinalizeDisplay(List<FishCaughtDisplay> displayList)
    {
        displayList = base.FinalizeDisplay(displayList);
        foreach (FishCaughtDisplay disp in displayList)
        {
            HashSet<string> canCatchIn = [];
            foreach ((LocationInfo locInfo, SpawnFishData spawn) in disp.Info.FromFishing)
            {
                if (spawn.RequireMagicBait)
                    continue;
                Season? season = spawn.Season;
                if (season != null && season != Game1.GetSeasonForLocation(locInfo.Location))
                    continue;
                string? condition = spawn.Condition;
                if (condition != null && !GameQueryHelper.ContextLocationCheckNoRandom(condition, locInfo.Location))
                    continue;
                if (disp.Info.FishReq is FishSpawnReq spawnReq)
                {
                    if (
                        spawnReq.CrabPotGroups == null
                        && spawnReq.Rain != null
                        && spawnReq.Rain != locInfo.Location.IsRainingHere()
                    )
                        continue;
                }
                canCatchIn.Add(locInfo.Location.DisplayName ?? locInfo.LocationId);
            }
            // mines fish hardcoding
            switch (disp.Info.Datum.QualifiedItemId)
            {
                case "(O)158":
                    canCatchIn.Add(I18n.Location_Mines_20());
                    break;
                case "(O)161":
                    canCatchIn.Add(I18n.Location_Mines_60());
                    break;
                case "(O)162":
                    canCatchIn.Add(I18n.Location_Mines_100());
                    break;
            }
            List<string> canCatchInLst = canCatchIn.ToList();
            canCatchInLst.Sort();
            disp.SetCanCatchIn(canCatchInLst);
        }
        return displayList;
    }

    protected override List<FishCaughtDisplay> SortAllDisplay(List<FishCaughtDisplay> displayList)
    {
        if (SortMode == SORTMODE_DEFAULT)
        {
            return displayList
                .OrderBy(static disp =>
                    (
                        (disp.canCatchIn?.Any() ?? false) ? -int.MaxValue : 0,
                        disp.Info.Datum.Category,
                        disp.Info.Datum.QualifiedItemId
                    )
                )
                .ToList();
        }
        return base.SortAllDisplay(displayList);
    }
}
