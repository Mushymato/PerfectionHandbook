using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
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
    public virtual Item ReprItem => Info.ReprItem;

    protected CountMode countMode = CountMode.Owned;

    protected int completedCount = 0;

    [Notify]
    protected int count = OwnedCount;

    public virtual Color DisplayTint => Count > 0 ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public virtual bool HasCount => Count > 0;

    public float DisplayShadow => DisplayTint == HandbookContext.ActiveColor ? 0.35f : 0f;

    [Notify]
    private bool isHovered = false;

    public float DisplayScale => IsHovered ? 1.1f : 1f;

    public virtual SDUITooltipData? Tooltip => new(GetTooltipDesc(), Info.Datum.DisplayName, ReprItem);

    public virtual bool Needed => completedCount == 0;

    public abstract void SetStatus(Farmer who);

    public virtual void SetCountMode(CountMode countMode)
    {
        this.countMode = countMode;
        UpdateCount();
        OnPropertyChanged(new(nameof(DisplayTint)));
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

    public bool SearchMatch(string txt) => Info.SearchMatch(txt);
}

public abstract partial class AbstractItemCountContext<TDisplay> : AbstractPageListContext<TDisplay>
    where TDisplay : AbstractItemCountDisplay
{
    public AbstractItemCountContext(
        IGoalContext goalCtx,
        bool canToggleCountMode = true,
        CountMode defaultCountMode = CountMode.Owned
    )
        : base(goalCtx, canToggleCountMode: canToggleCountMode)
    {
        if (defaultCountMode != CountMode.Owned && canToggleCountMode)
            ClickToggleCount();
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
        return SortAllDisplay(displayList);
    }

    protected virtual bool ShouldInclude(ItemInfo itemInfo) => throw new NotImplementedException(nameof(ShouldInclude));

    protected virtual TDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) =>
        throw new NotImplementedException(nameof(MakeDisplay));

    protected virtual IReadOnlyList<TDisplay> SortAllDisplay(List<TDisplay> displayList) =>
        displayList.OrderBy(static disp => (disp.Info.Datum.Category, disp.Info.Datum.QualifiedItemId)).ToList();

    [Notify]
    protected TDisplay? hovered = null;

    public bool HasHovered => Hovered != null;

    public virtual void HoveredEnter(TDisplay display)
    {
        Hovered?.IsHovered = false;
        Hovered = display;
        display.IsHovered = true;
    }

    public virtual string CompleteCountToggleText => string.Empty;

    [Notify]
    public string countToggleText = I18n.Ui_CountingOwned();

    private CountMode countMode = CountMode.Owned;

    public void ClickToggleCount()
    {
        switch (countMode)
        {
            case CountMode.Owned:
                countMode = CountMode.Completed;
                CountToggleText = CompleteCountToggleText;
                break;
            case CountMode.Completed:
                countMode = CountMode.Owned;
                CountToggleText = I18n.Ui_CountingOwned();
                break;
        }
        foreach (TDisplay display in AllDisplay)
        {
            display.SetCountMode(countMode);
        }
    }
}
