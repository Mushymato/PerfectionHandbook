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
    Func<IGoalContext, object?> GetPageCtx
) : IGoalContext
{
    private static readonly IReadOnlyList<GoalFulfillment> Empty = [];
    public object? PageCtx => GetPageCtx(this);
    public IReadOnlyList<GoalFulfillment> Fulfillments => Empty;
}
