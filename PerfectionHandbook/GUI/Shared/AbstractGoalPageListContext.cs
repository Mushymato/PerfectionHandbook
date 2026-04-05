using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public abstract partial class AbstractGoalPageListContext<TDisplay>
    where TDisplay : IPageDisplayEntry
{
    public readonly GoalContext GoalCtx;

    public readonly IReadOnlyList<TDisplay> AllDisplay;

    public AbstractGoalPageListContext(GoalContext goalCtx)
    {
        GoalCtx = goalCtx;
        AllDisplay = MakeAllDisplay();
        UpdateDisplayingFarmer(goalCtx.MyFulfillment.Who);
    }

    [Notify]
    public string searchText = string.Empty;

    [Notify]
    private Farmer? displayingFarmer = null;

    [Notify]
    private bool showNeeded = true;

    public void ClickMyFulfilment()
    {
        UpdateDisplayingFarmer(GoalCtx.MyFulfillment.Who);
    }

    public void ClickBestFulfilment()
    {
        UpdateDisplayingFarmer(GoalCtx.BestFulfillment?.Who);
    }

    protected virtual void UpdateDisplayingFarmer(Farmer? who)
    {
        if (who == null)
            return;
        if (who != DisplayingFarmer)
        {
            DisplayingFarmer = who;
            ShowNeeded = true;
        }
        else
        {
            ShowNeeded = !ShowNeeded;
        }
        UpdateDisplayStatus();
    }

    private void UpdateDisplayStatus()
    {
        if (DisplayingFarmer != null)
            foreach (TDisplay display in AllDisplay)
                display.SetStatus(DisplayingFarmer);
    }

    protected abstract IReadOnlyList<TDisplay> MakeAllDisplay();

    public IReadOnlyList<TDisplay> FilteredDisplay
    {
        get
        {
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            bool showNeed = ShowNeeded;
            string txt = SearchText;
            int shownCnt = HandbookContext.MAX_SHOWN;
            List<TDisplay> filteredDisplay = [];
            foreach (TDisplay display in AllDisplay)
            {
                if (display.Needed != showNeed)
                    continue;
                if (!display.Info.SearchMatch(txt))
                    continue;
                filteredDisplay.Add(display);
                shownCnt--;
                if (shownCnt <= 0)
                    break;
            }
            return filteredDisplay;
        }
    }
}
