using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI.Shared;

public interface IGoalContext
{
    Farmer Who { get; }
    PlayerOwned OwnedInfo { get; }
    public string PageName { get; }
    public string SummaryText { get; }
    string DisplayName { get; }
    ParsedItemData DisplayIcon { get; }
    public object? PageCtx { get; }
    public IReadOnlyList<GoalFulfillment> Fulfillments { get; }
}
