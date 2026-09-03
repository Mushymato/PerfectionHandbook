using System.ComponentModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public enum CountMode
{
    Owned,
    Completed,
}

public abstract partial record AbstractItemCountDisplay(ItemInfo Info, int OwnedCount) : IPageDisplayEntry
{
    public virtual string FocusableTag { get; } = string.Empty;

    public override int GetHashCode() => Info.Datum.QualifiedItemId.GetHashCode();

    public virtual Item ReprItem => Info.ReprItem;

    protected CountMode countMode = CountMode.Owned;

    protected int completedCount = 0;

    [Notify]
    protected int count = OwnedCount;

    public virtual Color DisplayTint => Count > 0 ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public virtual bool HasCount => Count > 0;

    public virtual float DisplayShadow => DisplayTint == HandbookContext.ActiveColor ? 0.35f : 0f;

    [Notify]
    private bool isHovered = false;

    [Notify]
    private bool isLocked = false;

    public float DisplayScale => IsHovered ? 1.125f : 1f;

    public Color BorderTint => IsHovered && IsLocked ? Color.White : Color.Transparent;

    protected SDUITooltipData? toolitp = null;
    public virtual SDUITooltipData? Tooltip => toolitp ??= new(GetTooltipDesc(), Info.Datum.DisplayName, ReprItem);

    public abstract ReminderEntry? Reminder { get; }

    public virtual bool Needed => completedCount == 0;

    public virtual void SetStatus(Farmer who) { }

    public virtual void SetCountMode(CountMode countMode)
    {
        this.countMode = countMode;
        UpdateCount();
        OnPropertyChanged(new(nameof(DisplayTint)));
        OnPropertyChanged(new(nameof(DisplayShadow)));
    }

    public void UpdateCount()
    {
        switch (countMode)
        {
            case CountMode.Completed:
                Count = completedCount;
                break;
            case CountMode.Owned:
                Count = OwnedCount;
                break;
        }
    }

    public virtual string GetTooltipDesc()
    {
        if (OwnedCount == 0)
            return Info.Datum.Description;
        return string.Concat(
            Info.Datum.Description,
            Environment.NewLine,
            Environment.NewLine,
            I18n.Ui_OwnedCount(OwnedCount)
        );
    }

    public virtual bool SearchMatch(string txt) => Info.SearchMatch(txt);

    public bool ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);
}

public abstract partial class AbstractItemCountContext<TDisplay> : AbstractPageListContext<TDisplay>
    where TDisplay : AbstractItemCountDisplay
{
    public override bool HasSortModes => true;

    public AbstractItemCountContext(
        IGoalContext goalCtx,
        bool canToggleNeeded = true,
        bool canToggleCountMode = true,
        CountMode defaultCountMode = CountMode.Owned,
        double itemPerPageModifier = 1
    )
        : base(
            goalCtx,
            canToggleNeeded: canToggleNeeded,
            canToggleCountMode: canToggleCountMode,
            itemPerPageModifier: itemPerPageModifier
        )
    {
        PropertyChanged += OnPropertyChanged_CountMode;
        CountMode = defaultCountMode;
    }

    protected override IReadOnlyList<TDisplay> MakeAllDisplay()
    {
        List<TDisplay> displayList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            if (!ShouldInclude(itemInfo))
                continue;
            int ownedCount = 0;
            if (GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(itemInfo.Datum.QualifiedItemId, out OwnedItemGroup? group))
                ownedCount = group.CountRepr.ReprStack;
            displayList.Add(MakeDisplay(itemInfo, ownedCount));
        }
        return FinalizeDisplay(displayList);
    }

    protected virtual bool ShouldInclude(ItemInfo itemInfo) => throw new NotImplementedException(nameof(ShouldInclude));

    protected virtual TDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) =>
        throw new NotImplementedException(nameof(MakeDisplay));

    protected virtual List<TDisplay> FinalizeDisplay(List<TDisplay> displayList)
    {
        RemindersHUD remindersHUD = MenuHandler.Reminders;
        foreach (TDisplay display in displayList)
        {
            if (display.Reminder is ReminderEntry entry)
            {
                entry.Active = remindersHUD.HasEntry(entry);
            }
        }
        return displayList;
    }

    protected override List<TDisplay> SortAllDisplay(List<TDisplay> displayList)
    {
        return SortMode switch
        {
            SORTMODE_DEFAULT => displayList
                .OrderBy(static disp => (disp.Info.Datum.Category, disp.Info.Datum.QualifiedItemId))
                .ToList(),
            SORTMODE_NAME => displayList
                .OrderBy(static disp => disp.Info.Datum.DisplayName, ModEntry.displayStringComparer)
                .ToList(),
            SORTMODE_COUNT => displayList.OrderByDescending(static disp => disp.Count).ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }

    [Notify]
    protected TDisplay? hovered = null;

    [Notify]
    private bool hoverable = true;

    public bool HasHovered => Hovered != null;

    public virtual void HoveredEnter(TDisplay display)
    {
        if (Hoverable)
        {
            DoHoveredEnter(display);
        }
    }

    public virtual void HoveredClick(TDisplay display)
    {
        if (Hoverable)
        {
            DoHoveredEnter(display);
            LockHoverable(display);
        }
        else
        {
            if (display == Hovered)
            {
                UnlockHoverable(display);
            }
            else
            {
                DoHoveredEnter(display);
                LockHoverable(display);
            }
        }
    }

    protected virtual void DoHoveredEnter(TDisplay display)
    {
        Hovered?.IsHovered = false;
        Hovered = display;
        display.IsHovered = true;
    }

    public virtual void LockHoverable(TDisplay display)
    {
        Hoverable = false;
        display.IsLocked = true;
        Game1.playSound("dwop");
        MenuHandler.Handbook_FocusOnTaggedView("side-panel-title");
    }

    public virtual void UnlockHoverable(TDisplay display)
    {
        DoUnlockHoverable(display);
        Game1.playSound("dwoop");
    }

    private void DoUnlockHoverable(TDisplay display)
    {
        Hoverable = true;
        display.IsLocked = false;
        MenuHandler.Handbook_FocusOnTaggedView(display.FocusableTag);
    }

    public virtual string OwnedCountToggleText => I18n.Ui_CountingOwned();
    public virtual string CompleteCountToggleText => string.Empty;

    [Notify]
    private CountMode countMode = CountMode.Owned;
    public int CountModeIndex
    {
        get => CountMode == CountMode.Completed ? 1 : 0;
        set =>
            CountMode = value switch
            {
                1 => CountMode.Completed,
                _ => CountMode.Owned,
            };
    }

    private void OnPropertyChanged_CountMode(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CountMode))
            SetAllCountMode();
    }

    private void SetAllCountMode()
    {
        foreach (TDisplay display in AllDisplay)
        {
            display.SetCountMode(countMode);
        }
        if (SortMode == SORTMODE_COUNT)
        {
            ReSortFilteredDisplay();
        }
    }

    public override bool TryExitPage()
    {
        if (Hovered != null)
        {
            bool locked = Hovered.IsLocked;
            DoUnlockHoverable(Hovered);
            Hovered.IsHovered = false;
            Hoverable = true;
            Hovered = null;
            return !locked;
        }
        return base.TryExitPage();
    }
}
