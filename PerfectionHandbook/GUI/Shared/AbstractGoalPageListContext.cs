using Microsoft.Xna.Framework;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley.Extensions;

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
        ShowNeeded = !goalCtx.Fulfillments[0].Filled;
        UpdateDisplayingFulfillment(goalCtx.Fulfillments[0]);
    }

    public string SearchText
    {
        get => field;
        set
        {
            if (!field.EqualsIgnoreCase(value))
            {
                field = value;
                filteredDisplay = null;
                OnPropertyChanged(new(nameof(SearchText)));
                OnPropertyChanged(new(nameof(FilteredDisplayPaginated)));
            }
        }
    } = string.Empty;

    public bool ShowNeeded
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                filteredDisplay = null;
                OnPropertyChanged(new(nameof(ShowNeeded)));
                OnPropertyChanged(new(nameof(FilteredDisplayPaginated)));
            }
        }
    } = true;

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

    public void ClickShowNeeded()
    {
        filteredDisplay = null;
        ShowNeeded = !ShowNeeded;
        OnPropertyChanged(new(nameof(FilteredDisplay)));
    }

    public void ClickFulfilment(GoalFulfillment fulfillment)
    {
        UpdateDisplayingFulfillment(fulfillment);
    }

    private GoalFulfillment? displayingFulfillment = null;

    protected virtual void UpdateDisplayingFulfillment(GoalFulfillment fulfillment)
    {
        if (displayingFulfillment != fulfillment)
        {
            filteredDisplay = null;
            displayingFulfillment = fulfillment;
            if (fulfillment.Who != null)
                foreach (TDisplay display in AllDisplay)
                    display.SetStatus(fulfillment.Who);
            foreach (GoalFulfillment eachful in GoalCtx.Fulfillments)
                eachful.DisplayTint = eachful == displayingFulfillment ? Color.White : Color.Transparent;
            OnPropertyChanged(new(nameof(FilteredDisplayPaginated)));
        }
    }

    protected abstract IReadOnlyList<TDisplay> MakeAllDisplay();
    private List<TDisplay>? filteredDisplay = null;
    protected List<TDisplay> FilteredDisplay
    {
        get
        {
            if (this.filteredDisplay != null)
                return this.filteredDisplay;
            bool showNeed = ShowNeeded;
            string txt = SearchText;
            List<TDisplay> filteredDisplay = [];
            foreach (TDisplay display in AllDisplay)
            {
                if (display.Needed != showNeed)
                    continue;
                if (!display.SearchMatch(txt))
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
            if (filteredDisplay.Count == 0)
                return filteredDisplay;
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
