using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record GoalContext(
    IGoal Goal,
    GoalFulfillment MyFulfillment,
    GoalFulfillment? BestFulfillment,
    PlayerOwned OwnedInfo
)
{
    public static GoalContext Make(Farmer who, IGoal goal, PlayerOwned ownedInfo)
    {
        GoalFulfillment myFulfillment = goal.GetFulfillment(who);
        if (goal.IsShared)
        {
            return new(goal, myFulfillment, null, ownedInfo);
        }
        return new(goal, myFulfillment, goal.GetBestFulfillment(who, myFulfillment), ownedInfo);
    }

    public bool ShowBestFulfillment => BestFulfillment != null && BestFulfillment.Who != MyFulfillment.Who;
    public string PageName => Goal.GetType().Name;
    public object? PageCtx => field ??= Goal.GetPageContext(this);
}

public sealed partial class HandbookContext(Farmer who)
{
    public const int MAX_SHOWN = 15 * 80;

    private readonly PlayerOwned playerOwned = ItemOwnedCache.GetPlayerOwned();
    public IReadOnlyList<GoalContext> PerfectionGoals
    {
        get
        {
            if (field != null)
                return field;
            List<GoalContext> goalContexts = [];
            foreach (IGoal goal in Goals.PerfectionGoals)
            {
                goalContexts.Add(GoalContext.Make(who, goal, playerOwned));
            }
            field = goalContexts;
            return goalContexts;
        }
    } = null;

    [Notify]
    private GoalContext? selectedGoalCtx = null;
    public string PageName => SelectedGoalCtx?.PageName ?? "Main";

    public void ChangePage(GoalContext goalCtx)
    {
        if (goalCtx.PageCtx != null)
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
