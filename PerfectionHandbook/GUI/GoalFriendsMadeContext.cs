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
                return HandbookContext.InactiveColor;
            return HandbookContext.ActiveColor;
        }
    }
    public bool Needed => CurrentFriendship == null || CurrentFriendship.Points < NpcInfo.MaxPoints;
    public float FriendshipFill =>
        100f * MathF.Min(CurrentFriendship?.Points ?? 0, NpcInfo.MaxPoints) / NpcInfo.MaxPoints;
    public string FriendshipFillLayout => $"{FriendshipFill}% stretch";

    public int HeartLevel => (CurrentFriendship?.Points ?? 0) / NPC.friendshipPointsPerHeartLevel;
    public string FriendshipPointDisplay =>
        I18n.Ui_Fulfillment_Dipslay(CurrentFriendship?.Points ?? 0, NpcInfo.MaxPoints);

    public readonly string DisplayName = NpcInfo.Chara?.displayName ?? TokenParser.ParseText(NpcInfo.Data.DisplayName);
    public string ScreenRead => $"{DisplayName} {FriendshipPointDisplay}";
    public ReminderEntry? Reminder => throw new NotImplementedException();

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
    public override bool HasSortModes => true;
    protected override string[] ValidSortModes => [SORTMODE_NAME, SORTMODE_COUNT];
    public override string SortMode
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                ReSortFilteredDisplay();
            }
        }
    } = SORTMODE_NAME;

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
        return friendDisplay;
    }

    protected override List<FriendDisplay> SortAllDisplay(List<FriendDisplay> displayList)
    {
        return SortMode switch
        {
            SORTMODE_NAME => displayList.OrderBy(static disp => disp.DisplayName).ToList(),
            SORTMODE_COUNT => displayList.OrderByDescending(static disp => disp.FriendshipFill).ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }
}
