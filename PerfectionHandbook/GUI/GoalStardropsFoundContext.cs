using PerfectionHandbook.GUI.Shared;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record StardropsFoundDisplay(string FoundFlag) : IPageDisplayEntry
{
    public string Description = I18n.GetByKey(string.Concat("Stardrop.Desc.", FoundFlag));
    private bool needed = false;
    public bool Needed => needed;

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return Description.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        needed = !who.hasOrWillReceiveMail(FoundFlag);
    }
}

public sealed class GoalStardropsFoundContext(GoalContext goalCtx)
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
