using System.ComponentModel;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
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

public sealed class GoalFishCaughtContext : AbstractGoalPageListContext<FishCaughtDisplay>
{
    public GoalFishCaughtContext(GoalContext GoalCtx)
        : base(GoalCtx)
    {
        PropertyChanged += OnUpdateFilteredDisplay;
    }

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

    public IReadOnlyList<FishCaughtDisplay> FilteredDisplay_InSeason
    {
        get { return []; }
    }

    public IReadOnlyList<FishCaughtDisplay> FilteredDisplay_OffSeason
    {
        get { return []; }
    }

    private void OnUpdateFilteredDisplay(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FilteredDisplay")
        {
            OnPropertyChanged(new(nameof(FilteredDisplay_InSeason)));
            OnPropertyChanged(new(nameof(FilteredDisplay_OffSeason)));
        }
    }
}
