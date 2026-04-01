using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record GoalContext(IGoal Goal, GoalFulfillment MyFulfillment, GoalFulfillment? BestFulfillment)
{
    public static GoalContext Make(Farmer who, IGoal goal)
    {
        GoalFulfillment myFulfillment = goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return new(goal, myFulfillment, null);
        }
        return new(goal, myFulfillment, goal.GetBestFulfillment(who, myFulfillment));
    }

    public bool ShowBestFulfillment => BestFulfillment != null && BestFulfillment.Who != MyFulfillment.Who;
}

public sealed class HandbookContext(Farmer who)
{
    public IReadOnlyList<GoalContext> PerfectionGoals
    {
        get
        {
            List<GoalContext> goalContexts = [];
            foreach (IGoal goal in Goals.PerfectionGoals)
            {
                goalContexts.Add(GoalContext.Make(who, goal));
            }
            return goalContexts;
        }
    }
}
