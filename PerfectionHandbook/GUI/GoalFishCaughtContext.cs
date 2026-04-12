using System.Text;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record FishCaughtDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    public override bool Needed => Count == 0;
    private int biggestCatch = 0;
    private IReadOnlyList<string>? canCatchIn = null;
    public override Color DisplayTint =>
        canCatchIn != null ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

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
}

public sealed class GoalFishCaughtContext(GoalContext goalCtx)
    : AbstractItemCountContext<FishCaughtDisplay>(goalCtx, false)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsCatchableFish;

    protected override FishCaughtDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);

    protected override IReadOnlyList<FishCaughtDisplay> SortAllDisplay(List<FishCaughtDisplay> displayList)
    {
        return displayList
            .OrderBy(static disp =>
            {
                HashSet<string> canCatchIn = [];
                foreach (FishSourceInfo fishSourceInfo in disp.Info.FromFishing)
                {
                    if (!Game1.player.locationsVisited.Contains(fishSourceInfo.Location.NameOrUniqueName))
                        continue;
                    Season? season = fishSourceInfo.Spawn?.Season;
                    if (season != null && season != Game1.GetSeasonForLocation(fishSourceInfo.Location))
                        continue;
                    string? condition = fishSourceInfo.Spawn?.Condition;
                    if (
                        condition != null
                        && !GameQueryHelper.ContextLocationCheckNoRandom(condition, fishSourceInfo.Location)
                    )
                        continue;
                    if (disp.Info.FishReq is FishSpawnReq spawnReq)
                    {
                        if (
                            spawnReq.CrabPotGroups == null
                            && spawnReq.Rain != null
                            && spawnReq.Rain != fishSourceInfo.Location.IsRainingHere()
                        )
                            continue;
                    }
                    canCatchIn.Add(fishSourceInfo.Location.DisplayName ?? fishSourceInfo.LocationId);
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
                return (
                    canCatchIn.Any() ? -int.MaxValue : 0,
                    disp.Info.Datum.Category,
                    disp.Info.Datum.QualifiedItemId
                );
            })
            .ToList();
    }
}
