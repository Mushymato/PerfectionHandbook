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
    public Item ReprItem = OwnedRepr ?? new(Info.ReprItem.getOne());

    public SDUITooltipData Tooltip => new(GetTooltipDesc(), Info.Datum.DisplayName, ReprItem);

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
            I18n.Ui_OwnedCount(OwnedRepr.Stack)
        );
    }

    public bool SearchMatch(string txt) => Info.SearchMatch(txt);
}

public abstract class AbstractItemCountContext<TDisplay>(GoalContext goalCtx)
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
        return displayList;
    }

    protected abstract bool ShouldInclude(ItemInfo itemInfo);

    protected virtual ReprObject? GetReprObject(ItemInfo itemInfo)
    {
        ReprObject? ownedRepr = null;
        if (GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(itemInfo.Datum.QualifiedItemId, out OwnedItemGroup? group))
        {
            ownedRepr = group.CountRepr;
        }
        return ownedRepr;
    }

    protected abstract TDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr);
}
