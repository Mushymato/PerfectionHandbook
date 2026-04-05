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

    [Notify]
    private int scrollPage = 1;
    public float ScrollProgress
    {
        get => field;
        set
        {
            bool changed = false;
            if (value <= 0 && scrollPage > 1)
            {
                scrollPage--;
                field = 0.9999f;
                changed = true;
            }
            else if (value >= 1 && (scrollPage * HandbookContext.MAX_SHOWN < FilteredDisplay.Count))
            {
                scrollPage++;
                field = 0f;
                changed = true;
            }
            if (changed)
            {
                OnPropertyChanged(new(nameof(ScrollPage)));
                OnPropertyChanged(new(nameof(ScrollProgress)));
            }
        }
    }

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
        filteredDisplay = null;
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
    private List<TDisplay>? filteredDisplay = null;
    protected List<TDisplay> FilteredDisplay
    {
        get
        {
            if (this.filteredDisplay != null)
                return this.filteredDisplay;
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            bool showNeed = ShowNeeded;
            string txt = SearchText;
            List<TDisplay> filteredDisplay = [];
            foreach (TDisplay display in AllDisplay)
            {
                if (display.Needed != showNeed)
                    continue;
                if (!display.Info.SearchMatch(txt))
                    continue;
                filteredDisplay.Add(display);
            }
            this.filteredDisplay = filteredDisplay;
            return this.filteredDisplay;
        }
    }

    public IReadOnlyList<TDisplay> FilteredDisplayPaginated
    {
        get
        {
            List<TDisplay> filteredDisplay = FilteredDisplay;
            int actualPage = ScrollPage - 1;
            int nextPageSize = Math.Min(
                HandbookContext.MAX_SHOWN,
                filteredDisplay.Count - actualPage * HandbookContext.MAX_SHOWN
            );
            if (nextPageSize == 0)
                return [];
            return filteredDisplay.GetRange(actualPage * HandbookContext.MAX_SHOWN, nextPageSize);
        }
    }
}
