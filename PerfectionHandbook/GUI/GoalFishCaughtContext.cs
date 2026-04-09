using System.Text;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record FishCaughtDisplay(ItemInfo Info, ReprObject? OwnedRepr) : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed => Count == 0;
    private int biggestCatch = 0;
    private IReadOnlyList<string>? canCatchIn = null;

    public override void SetStatus(Farmer who)
    {
        if (who.fishCaught.TryGetValue(Info.Datum.QualifiedItemId, out int[] pair))
        {
            Count = pair[0];
            biggestCatch = pair[1];
        }
        else
        {
            Count = 0;
            biggestCatch = 0;
        }
        OwnedRepr?.SetReprStack(Count);
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public void SetCanCatchIn(IReadOnlyList<string> canCatchIn)
    {
        this.canCatchIn = canCatchIn.Any() ? canCatchIn : null;
        DisplayTint = this.canCatchIn != null ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;
    }

    private static readonly StringBuilder sb = new();

    public override string GetTooltipDesc()
    {
        sb.Append(Info.Datum.Description);
        if (OwnedRepr != null && Count != 0)
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
}

public sealed class GoalFishCaughtContext(GoalContext goalCtx) : AbstractItemCountContext<FishCaughtDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsCatchableFish;

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    protected override FishCaughtDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(itemInfo, ownedRepr);

    protected override IReadOnlyList<FishCaughtDisplay> SortAllDisplay(List<FishCaughtDisplay> displayList)
    {
        return displayList
            .OrderBy(static disp =>
            {
                List<string> canCatchIn = [];
                foreach (FishSourceInfo fishSourceInfo in disp.Info.FromFishing)
                {
                    if (!Game1.player.locationsVisited.Contains(fishSourceInfo.Location!.NameOrUniqueName))
                        continue;
                    Season? season = fishSourceInfo.Spawn?.Season;
                    if (season != null && season != Game1.GetSeasonForLocation(fishSourceInfo.Location))
                        continue;
                    string? condition = fishSourceInfo.Spawn?.Condition;
                    if (
                        condition != null
                        && !GameQueryHelper.GSQCheckNoRandom(
                            condition.Replace(" Here ", " Target "),
                            fishSourceInfo.Location
                        )
                    )
                        continue;
                    if (disp.Info.FishReq is FishSpawnReq spawnReq)
                    {
                        if (
                            spawnReq.CrabPotGroups == null
                            && spawnReq.Rain != null
                            && spawnReq.Rain != (fishSourceInfo.Location?.IsRainingHere())
                        )
                            continue;
                    }
                    canCatchIn.Add(fishSourceInfo.Location?.DisplayName ?? fishSourceInfo.LocationId);
                }
                disp.SetCanCatchIn(canCatchIn);
                return (
                    canCatchIn.Any() ? -int.MaxValue : 0,
                    disp.Info.Datum.Category,
                    disp.Info.Datum.QualifiedItemId
                );
            })
            .ToList();
    }
}
