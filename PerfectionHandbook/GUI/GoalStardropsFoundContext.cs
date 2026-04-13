using PerfectionHandbook.GUI.Shared;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record StardropsFoundDisplay(string FoundFlag, string Desc) : IPageDisplayEntry
{
    public bool Needed => throw new NotImplementedException();

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return Desc.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        throw new NotImplementedException();
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
        return [];
    }
}
