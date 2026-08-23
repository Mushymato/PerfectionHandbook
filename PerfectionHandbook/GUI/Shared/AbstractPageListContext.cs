using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI.Shared;

public abstract partial class AbstractPageListContext<TDisplay> : IPageContext
    where TDisplay : IPageDisplayEntry
{
    public readonly IGoalContext GoalCtx;

    public readonly IReadOnlyList<TDisplay> AllDisplay;

    public readonly bool CanToggleNeeded = true;
    public readonly bool CanToggleCountMode = false;
    public readonly bool CanPaginate = true;

    public const string SORTMODE_DEFAULT = "default";
    public const string SORTMODE_NAME = "name";
    public const string SORTMODE_COUNT = "count";

    public virtual bool HasSortModes => false;
    protected virtual string[] ValidSortModes => [SORTMODE_DEFAULT, SORTMODE_NAME, SORTMODE_COUNT];
    public virtual string SortMode
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                ReSortFilteredDisplay();
            }
        }
    } = SORTMODE_DEFAULT;
    public virtual StringSpinBoxViewModel SortModeCtx =>
        new(() => SortMode, (value) => SortMode = value, ValidSortModes, "ui.sort-mode.");

    protected void ReSortFilteredDisplay()
    {
        filteredDisplay = null;
        UpdateFilteredDisplayPaginated();
    }

    public AbstractPageListContext(
        IGoalContext pageCtx,
        bool canToggleNeeded = true,
        bool canToggleCountMode = false,
        bool canPaginate = true
    )
    {
        GoalCtx = pageCtx;
        AllDisplay = MakeAllDisplay();
        CanToggleNeeded = canToggleNeeded;
        CanToggleCountMode = canToggleCountMode;
        CanPaginate = canPaginate;

        if (pageCtx.Fulfillments.Any())
        {
            NeededIndex = pageCtx.Fulfillments[0].Filled ? 1 : 0;
            UpdateDisplayingFulfillment(pageCtx.Fulfillments[0]);
        }
        else
        {
            NeededIndex = 0;
            CanToggleNeeded = false;
            UpdateAllStatus(pageCtx.Who);
        }
    }

    public int PrimaryItemCount
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(new(nameof(PrimaryItemCount)));
                OnPropertyChanged(new(nameof(HasNextPage)));
                UpdateFilteredDisplayPaginated();
            }
        }
    }

    private int rowPerPage = ModEntry.config.RowPerPage;

    private int GetItemPerPage()
    {
        return rowPerPage * (PrimaryItemCount <= 0 ? 10 : PrimaryItemCount);
    }

    public virtual string SearchText
    {
        get => field;
        set
        {
            if (!field.EqualsIgnoreCase(value))
            {
                field = value;
                filteredDisplay = null;
                OnPropertyChanged(new(nameof(SearchText)));
                UpdateFilteredDisplayPaginated();
            }
        }
    } = string.Empty;

    public int NeededIndex
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                filteredDisplay = null;
                OnPropertyChanged(new(nameof(NeededIndex)));
                UpdateFilteredDisplayPaginated();
            }
        }
    } = 0;

    [Notify]
    private int scrollPage = 1;

    public bool HasPrevPage => ScrollPage > 1;

    [DependsOn(nameof(ScrollPage))]
    private bool HasNextPage => ScrollPage * GetItemPerPage() < FilteredDisplay.Count;

    private float scrollProgress;
    public float ScrollProgress
    {
        get => scrollProgress;
        set
        {
            if (Game1.options.gamepadControls)
                return;
            if (value <= 0 && PaginatePrev())
            {
                scrollProgress = 0.9999f;
                OnPropertyChanged(new(nameof(ScrollProgress)));
            }
            else if (value >= 1 && PaginateNext())
            {
                scrollProgress = 0.0001f;
                OnPropertyChanged(new(nameof(ScrollProgress)));
            }
        }
    }

    public bool PaginatePrev()
    {
        if (HasPrevPage)
        {
            ScrollPage--;
            UpdateFilteredDisplayPaginated();
            return true;
        }
        return false;
    }

    public bool PaginateNext()
    {
        if (HasNextPage)
        {
            ScrollPage++;
            UpdateFilteredDisplayPaginated();
            return true;
        }
        return false;
    }

    [DependsOn(nameof(HasNextPage))]
    public bool HasPagination => CanPaginate && (HasPrevPage || HasNextPage);
    public float PrevPaginateButtonOpacity => HasPrevPage ? 1f : 0.4f;

    [DependsOn(nameof(HasNextPage))]
    public float NextPaginateButtonOpacity => HasNextPage ? 1f : 0.4f;

    public bool HandleShoulderButtons(SButton button)
    {
        switch (button)
        {
            case SButton.LeftShoulder:
                PaginatePrev();
                return true;
            case SButton.RightShoulder:
                PaginateNext();
                return true;
        }
        return false;
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
            UpdateAllStatus(fulfillment.Who);
            foreach (GoalFulfillment eachful in GoalCtx.Fulfillments)
                eachful.DisplayTint = eachful == displayingFulfillment ? Color.White : Color.Transparent;
            UpdateFilteredDisplayPaginated();
        }
    }

    private void UpdateAllStatus(Farmer? who)
    {
        if (who != null)
            foreach (TDisplay display in AllDisplay)
                display.SetStatus(who);
    }

    protected abstract IReadOnlyList<TDisplay> MakeAllDisplay();

    protected virtual List<TDisplay> SortAllDisplay(List<TDisplay> displayList) => displayList;

    protected List<TDisplay>? filteredDisplay = null;
    public List<TDisplay> FilteredDisplay
    {
        get
        {
            if (this.filteredDisplay != null)
                return this.filteredDisplay;
            bool showNeed = NeededIndex == 0;
            string txt = SearchText;
            List<TDisplay> filteredDisplay = [];
            foreach (TDisplay display in AllDisplay)
            {
                if (display.Needed != showNeed)
                    continue;
                if (!string.IsNullOrEmpty(txt) && !display.SearchMatch(txt))
                    continue;
                filteredDisplay.Add(display);
            }
            this.filteredDisplay = SortAllDisplay(filteredDisplay);
            OnPropertyChanged(new(nameof(HasNextPage)));
            return this.filteredDisplay;
        }
    }

    public readonly ObservableCollection<TDisplay> FilteredDisplayPaginated = [];

    protected void UpdateFilteredDisplayPaginated()
    {
        List<TDisplay> filtered = FilteredDisplay;
        if (filtered.Count == 0)
        {
            FilteredDisplayPaginated.Clear();
            return;
        }

        if (!CanPaginate)
        {
            FilteredDisplayPaginated.Clear();
            foreach (var display in FilteredDisplay)
                FilteredDisplayPaginated.Add(display);
            return;
        }

        if (MenuHandler.IsPreloading || PrimaryItemCount == 0)
        {
            FilteredDisplayPaginated.Clear();
            foreach (var display in filtered.GetRange(0, Math.Min(10, filtered.Count)))
                FilteredDisplayPaginated.Add(display);
            return;
        }

        int actualPage = ScrollPage - 1;
        int itemPerPage = GetItemPerPage();
        int startIdx = actualPage * itemPerPage;
        if (startIdx >= filtered.Count)
        {
            ScrollPage = 1;
            startIdx = 0;
        }
        int remainingCount = filtered.Count - startIdx;
        HashSet<TDisplay> matched = [];
        if (itemPerPage <= remainingCount)
        {
            matched.AddRange(filtered.GetRange(startIdx, itemPerPage));
        }
        else if (remainingCount > 0)
        {
            matched.AddRange(filtered.GetRange(startIdx, remainingCount));
        }
        FilteredDisplayPaginated.Clear();
        foreach (var display in matched)
            FilteredDisplayPaginated.Add(display);
    }

    public virtual bool TryOpenPage()
    {
        if (rowPerPage != ModEntry.config.RowPerPage)
        {
            rowPerPage = ModEntry.config.RowPerPage;
            UpdateFilteredDisplayPaginated();
        }
        return true;
    }

    public virtual bool TryExitPage() => true;
}
