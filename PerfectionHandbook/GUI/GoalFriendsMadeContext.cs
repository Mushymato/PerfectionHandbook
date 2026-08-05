using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed partial record FriendsMadeDisplay(NPCInfo NpcInfo) : IPageDisplayEntry
{
    [Notify]
    private bool isHovered = false;

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
    public ReminderEntry? Reminder { get; } = new ReminderEntry(ReminderEntryFactory.Kind_FriendsMade, NpcInfo.Name);

    public SDUISprite? MugShotSprite = NpcInfo.GetMugShot();

    public bool SearchMatch(string txt)
    {
        return DisplayName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        if (who.friendshipData.TryGetValue(NpcInfo.Name, out Friendship? friendship))
            CurrentFriendship = friendship;
        else
            CurrentFriendship = null;
    }

    public void ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);

    public IEnumerable<string> DebugEvents
    {
        get
        {
            foreach ((string key, EventInfo info) in NpcInfo.Events)
            {
                yield return $"{key} | {string.Join(' ', info.Preconditions)}";
            }
        }
    }
}

public partial class GoalFriendsMadeContext(IGoalContext goalCtx) : AbstractPageListContext<FriendsMadeDisplay>(goalCtx)
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

    protected override int GetItemPerPage()
    {
        return ModEntry.config.ItemPerPage / 2;
    }

    protected override IReadOnlyList<FriendsMadeDisplay> MakeAllDisplay()
    {
        List<FriendsMadeDisplay> friendDisplay = [];
        foreach (NPCInfo npcInfo in NPCInfoCache.Cache.Values)
        {
            if (!npcInfo.CountForPerfection)
                continue;
            FriendsMadeDisplay display = new(npcInfo);
            if (display.MugShotSprite != null)
                friendDisplay.Add(display);
        }
        return friendDisplay;
    }

    protected override List<FriendsMadeDisplay> SortAllDisplay(List<FriendsMadeDisplay> displayList)
    {
        return SortMode switch
        {
            SORTMODE_NAME => displayList.OrderBy(static disp => disp.DisplayName).ToList(),
            SORTMODE_COUNT => displayList.OrderByDescending(static disp => disp.FriendshipFill).ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }

    [Notify]
    protected FriendsMadeDisplay? hovered = null;

    public virtual void HoveredEnter(FriendsMadeDisplay display)
    {
        Hovered?.IsHovered = false;
        Hovered = display;
        display.IsHovered = true;
    }
}
