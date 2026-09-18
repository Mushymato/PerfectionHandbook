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
    Func<IGoalContext, IPageContext?> GetPageCtx
) : IGoalContext
{
    public string SummaryText => string.Empty;
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
