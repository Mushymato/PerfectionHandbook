using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record FishCaughtDisplay(ItemInfo Info) : IPageDisplayEntry
{
    public int Count { get; private set; } = 0;
    public int MaxLength { get; private set; } = 0;
    public bool Needed => Count == 0;

    public void SetStatus(Farmer who)
    {
        if (who.fishCaught.TryGetValue(Info.Datum.QualifiedItemId, out int[] pair))
        {
            Count = pair[0];
            MaxLength = pair[1];
        }
    }
}

public sealed class GoalFishCaughtContext(GoalContext GoalCtx) : AbstractGoalPageListContext<FishCaughtDisplay>(GoalCtx)
{
    protected override IReadOnlyList<FishCaughtDisplay> MakeAllDisplay()
    {
        List<FishCaughtDisplay> fishingList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            if (!itemInfo.IsCatchableFish)
                continue;
            fishingList.Add(new(itemInfo));
        }
        return fishingList;
    }
}
