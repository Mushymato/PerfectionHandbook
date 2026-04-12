using PerfectionHandbook.Models;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI.Shared;

public sealed record GoalContext(
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
    public object? PageCtx => field ??= Goal.GetPageContext(this);
    public string DisplayName => Goal.DisplayName;
    public ParsedItemData DisplayIcon => Goal.DisplayIcon;
}
