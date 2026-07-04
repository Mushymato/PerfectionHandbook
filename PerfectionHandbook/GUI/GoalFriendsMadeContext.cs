using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed partial record FriendDisplay(NPCInfo NpcInfo) : IPageDisplayEntry
{
    [Notify]
    private Friendship? currentFriendship = null;
    public Color DisplayTint
    {
        get
        {
            if (CurrentFriendship == null)
                return HandbookContext.HiddenColor;
            return HandbookContext.ActiveColor;
        }
    }
    public bool Needed => CurrentFriendship == null || CurrentFriendship.Points < NpcInfo.MaxPoints;
    public string FriendshipFillLayout =>
        CurrentFriendship == null
            ? "0% stretch"
            : $"{100f * MathF.Min(CurrentFriendship.Points, NpcInfo.MaxPoints) / NpcInfo.MaxPoints}% stretch";

    public readonly string DisplayName = NpcInfo.Chara?.displayName ?? TokenParser.ParseText(NpcInfo.Data.DisplayName);

    public SDUISprite? MugShotSprite = NpcInfo.GetMugShot();

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return DisplayName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        if (who.friendshipData.TryGetValue(NpcInfo.Name, out Friendship? friendship))
            CurrentFriendship = friendship;
        else
            CurrentFriendship = null;
    }
}

public sealed class GoalFriendsMadeContext(IGoalContext goalCtx) : AbstractPageListContext<FriendDisplay>(goalCtx)
{
    protected override IReadOnlyList<FriendDisplay> MakeAllDisplay()
    {
        List<FriendDisplay> friendDisplay = [];
        foreach (NPCInfo npcInfo in NPCInfoCache.Cache.Values)
        {
            if (!npcInfo.CountForPerfection)
                continue;
            FriendDisplay display = new(npcInfo);
            if (display.MugShotSprite != null)
                friendDisplay.Add(display);
        }
        friendDisplay = friendDisplay.OrderBy(npcInfo => TokenParser.ParseText(npcInfo.DisplayName)).ToList();
        return friendDisplay;
    }
}
