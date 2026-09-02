using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI.Shared;

public sealed partial record GoalContext(
    Farmer Who,
    IGoal Goal,
    PlayerOwned OwnedInfo,
    IReadOnlyList<GoalFulfillment> Fulfillments,
    string SummaryText
) : IGoalContext
{
    public static GoalContext Make(Farmer who, IGoal goal, PlayerOwned ownedInfo)
    {
        GoalFulfillment myFulfillment = goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return new(who, goal, ownedInfo, [myFulfillment], myFulfillment.DisplayText);
        }
        List<GoalFulfillment> allFulfilments = [];
        foreach (Farmer otherFarmer in Game1.getAllFarmers())
        {
            if (otherFarmer == who || !otherFarmer.isCustomized.Value)
                continue;
            allFulfilments.Add(goal.GetFulfillment(otherFarmer));
        }
        allFulfilments.Sort();
        GoalFulfillment bestFulfillment;
        if (allFulfilments.Any())
            bestFulfillment = myFulfillment.Percent >= allFulfilments[0].Percent ? myFulfillment : allFulfilments[0];
        else
            bestFulfillment = myFulfillment;
        allFulfilments.Insert(0, myFulfillment);
        return new(who, goal, ownedInfo, allFulfilments, bestFulfillment.DisplayText);
    }

    public string PageName => Goal.GetType().Name;
    private IPageContext? pageCtx = null;
    public IPageContext? PageCtx => pageCtx ??= Goal.GetPageContext(this);
    public string DisplayName => Goal.DisplayName;
    public ParsedItemData DisplayIcon => Goal.DisplayIcon;
    public bool Filled => Fulfillments[0].Filled;

    public void DisposePageCtx()
    {
        (pageCtx as IDisposable)?.Dispose();
        pageCtx = null;
    }
}
