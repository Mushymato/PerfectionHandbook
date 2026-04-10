using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI.Shared;

public abstract partial record AbstractItemCountDisplay(ItemInfo Info, ReprObject? OwnedRepr) : IPageDisplayEntry
{
    [Notify]
    public Color displayTint = OwnedRepr != null ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public Item ReprItem => OwnedRepr ?? Info.ReprItem;

    [Notify]
    protected int count = OwnedRepr?.ReprStack ?? 0;

    public virtual bool HasCount => Count > 0;

    public float DisplayShadow => DisplayTint == HandbookContext.ActiveColor ? 0.35f : 0f;

    [Notify]
    private bool isHovered = false;

    public float DisplayScale => IsHovered ? 1.1f : 1f;

    public virtual SDUITooltipData? Tooltip => new(GetTooltipDesc(), Info.Datum.DisplayName, ReprItem);

    public virtual bool Needed { get; protected set; } = false;

    public abstract void SetStatus(Farmer who);

    public virtual string GetTooltipDesc()
    {
        if (OwnedRepr == null)
            return Info.Datum.Description;
        return string.Concat(
            Info.Datum.Description,
            Environment.NewLine,
            Environment.NewLine,
            I18n.Ui_OwnedCount(OwnedRepr.ReprStack)
        );
    }

    public bool SearchMatch(string txt) => Info.SearchMatch(txt);
}

public abstract partial class AbstractItemCountContext<TDisplay>(GoalContext goalCtx)
    : AbstractGoalPageListContext<TDisplay>(goalCtx)
    where TDisplay : AbstractItemCountDisplay
{
    protected override IReadOnlyList<TDisplay> MakeAllDisplay()
    {
        List<TDisplay> displayList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            if (!ShouldInclude(itemInfo))
                continue;
            displayList.Add(MakeDisplay(itemInfo, GetReprObject(itemInfo)));
        }
        return SortAllDisplay(displayList);
    }

    protected virtual bool ShouldInclude(ItemInfo itemInfo) => throw new NotImplementedException(nameof(ShouldInclude));

    protected virtual ReprObject? GetReprObject(ItemInfo itemInfo)
    {
        ReprObject? ownedRepr = null;
        if (GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(itemInfo.Datum.QualifiedItemId, out OwnedItemGroup? group))
        {
            ownedRepr = group.CountRepr;
        }
        return ownedRepr;
    }

    protected virtual TDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        throw new NotImplementedException(nameof(MakeDisplay));

    protected virtual IReadOnlyList<TDisplay> SortAllDisplay(List<TDisplay> displayList) =>
        displayList.OrderBy(static disp => (disp.Info.Datum.Category, disp.Info.Datum.QualifiedItemId)).ToList();

    [Notify]
    protected TDisplay? hovered = null;

    public virtual void HoveredEnter(TDisplay display)
    {
        Hovered?.IsHovered = false;
        Hovered = display;
        display.IsHovered = true;
    }
}
