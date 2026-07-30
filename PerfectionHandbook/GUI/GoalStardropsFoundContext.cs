using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Reminders;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record StardropsFoundDisplay(string FoundFlag) : IPageDisplayEntry
{
    public string Description = I18n.GetByKey(string.Concat("Stardrop.Desc.", FoundFlag));
    private bool needed = false;
    public bool Needed => needed;
    public ReminderEntry? Reminder => throw new NotImplementedException();

    public bool SearchMatch(string txt)
    {
        return Description.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        needed = !who.hasOrWillReceiveMail(FoundFlag);
    }
}

public sealed class GoalStardropsFoundContext(IGoalContext goalCtx)
    : AbstractPageListContext<StardropsFoundDisplay>(goalCtx)
{
    private static readonly string[] StardropMailflags =
    [
        "CF_Fair",
        "CF_Fish",
        "CF_Mines",
        "CF_Sewer",
        "museumComplete",
        "CF_Spouse",
        "CF_Statue",
    ];

    protected override IReadOnlyList<StardropsFoundDisplay> MakeAllDisplay()
    {
        return StardropMailflags.Select(flag => new StardropsFoundDisplay(flag)).ToList();
    }
}
