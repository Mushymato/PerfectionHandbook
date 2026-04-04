using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record GoalContext(IGoal Goal, int Page, GoalFulfillment MyFulfillment, GoalFulfillment? BestFulfillment)
{
    public static GoalContext Make(Farmer who, IGoal goal, int page)
    {
        GoalFulfillment myFulfillment = goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return new(goal, page, myFulfillment, null);
        }
        return new(goal, page, myFulfillment, goal.GetBestFulfillment(who, myFulfillment));
    }

    public bool ShowBestFulfillment => BestFulfillment != null && BestFulfillment.Who != MyFulfillment.Who;

    public object? PageContext => Goal.GetPageContext(this);
}

public sealed partial class HandbookContext(Farmer who)
{
    public IReadOnlyList<GoalContext> PerfectionGoals
    {
        get
        {
            List<GoalContext> goalContexts = [];
            int page = 1;
            foreach (IGoal goal in Goals.PerfectionGoals)
            {
                goalContexts.Add(GoalContext.Make(who, goal, page));
                page++;
            }
            return goalContexts;
        }
    }

    [Notify]
    private GoalContext? selectedGoalCtx = null;
    public int Page => SelectedGoalCtx?.Page ?? 0;

    public void ChangePage(GoalContext goalCtx)
    {
        SelectedGoalCtx = goalCtx;
    }

    internal void CloseAction()
    {
        if (SelectedGoalCtx != null)
        {
            SelectedGoalCtx = null;
        }
        else
        {
            Game1.exitActiveMenu();
            Game1.player.forceCanMove();
        }
    }
}
