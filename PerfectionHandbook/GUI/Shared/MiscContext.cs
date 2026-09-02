using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI.Shared;

public sealed record MiscContext(
    Farmer Who,
    PlayerOwned OwnedInfo,
    string PageName,
    string DisplayName,
    ParsedItemData DisplayIcon,
    string SummaryText,
    Func<IGoalContext, IPageContext?> GetPageCtx
) : IGoalContext
{
    private static readonly IReadOnlyList<GoalFulfillment> Empty = [];
    private IPageContext? pageCtx = null;
    public IPageContext? PageCtx => pageCtx ??= GetPageCtx(this);
    public IReadOnlyList<GoalFulfillment> Fulfillments => Empty;
    public bool Filled => false;

    public void DisposePageCtx()
    {
        (pageCtx as IDisposable)?.Dispose();
        pageCtx = null;
    }
}
