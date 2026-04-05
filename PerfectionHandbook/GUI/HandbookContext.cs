using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record GoalContext(
    IGoal Goal,
    GoalFulfillment MyFulfillment,
    GoalFulfillment? BestFulfillment,
    PlayerOwned PlayerOwned
)
{
    public static GoalContext Make(Farmer who, IGoal goal, PlayerOwned playerOwned)
    {
        GoalFulfillment myFulfillment = goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return new(goal, myFulfillment, null, playerOwned);
        }
        return new(goal, myFulfillment, goal.GetBestFulfillment(who, myFulfillment), playerOwned);
    }

    public bool ShowBestFulfillment => BestFulfillment != null && BestFulfillment.Who != MyFulfillment.Who;
    public string PageName => Goal.GetType().Name;
    public object? PageContext => field ??= Goal.GetPageContext(this);
}

public sealed partial class HandbookContext(Farmer who)
{
    public const int MAX_SHOWN = 1000;

    private readonly PlayerOwned playerOwned = ItemOwnedCache.GetPlayerOwned(who);
    public IReadOnlyList<GoalContext> PerfectionGoals
    {
        get
        {
            List<GoalContext> goalContexts = [];
            foreach (IGoal goal in Goals.PerfectionGoals)
            {
                goalContexts.Add(GoalContext.Make(who, goal, playerOwned));
            }
            return goalContexts;
        }
    }

    [Notify]
    private GoalContext? selectedGoalCtx = null;
    public string PageName => SelectedGoalCtx?.PageName ?? "Main";

    public void ChangePage(GoalContext goalCtx)
    {
        if (goalCtx.PageContext != null)
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
