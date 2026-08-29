using System.Collections.ObjectModel;
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

public sealed record EventPreconditionInfoDisplay(EventPreconditionInfo Info, bool Status);

public sealed partial record EventInfoDisplay(EventInfo Info, string ForNPC, int RequiredFriendshipForNPC)
{
    [Notify]
    private bool hasSeen = false;

    [Notify]
    private bool isExpanded = false;

    public readonly int RequiredHeartLevelForNPC = RequiredFriendshipForNPC / NPC.friendshipPointsPerHeartLevel;
    public readonly bool HasRequiredFriendshipForNPC = RequiredFriendshipForNPC > -1;
    public readonly IReadOnlyList<EventPreconditionInfoDisplay> Preconds = Info
        .Preconditions.Select(precond => new EventPreconditionInfoDisplay(precond, precond.Evaluate(Info)))
        .ToList();

    internal bool Matches(string searchText)
    {
        return Info.HeaderText.ContainsIgnoreCase(searchText);
    }

    internal static EventInfoDisplay Make(EventInfo Info, string ForNPC)
    {
        return new(Info, ForNPC, Info.GetRequiredFriendship(ForNPC));
    }
}

public sealed partial record FriendsMadeDisplay(NPCInfo NpcInfo) : IPageDisplayEntry
{
    public override int GetHashCode() => NpcInfo.Name.GetHashCode();

    [Notify]
    private Friendship? currentFriendship = null;

    [Notify]
    private EventInfoDisplay? currentEventInfo = null;
    private readonly Stack<EventInfoDisplay> eventInfoStack = [];
    public bool HasCurrentEventInfo => CurrentEventInfo != null;

    public Color DisplayTint => CurrentFriendship == null ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
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
    public readonly IReadOnlyList<EventInfoDisplay> EventDisplays = NpcInfo
        .Events.Values.Select(ei => EventInfoDisplay.Make(ei, NpcInfo.Name))
        .OrderBy(static eid =>
            (eid.HasRequiredFriendshipForNPC ? eid.RequiredFriendshipForNPC : int.MaxValue, eid.Info.EventId)
        )
        .ToList();

    public readonly ObservableCollection<EventInfoDisplay> EventDisplaysFiltered = [];

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
        foreach (EventInfoDisplay eventDisp in EventDisplays)
        {
            eventDisp.HasSeen = who.eventsSeen.Contains(eventDisp.Info.EventId);
        }
    }

    public bool ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);

    internal void SearchEvents(string searchText)
    {
        if (CurrentEventInfo != null)
        {
            ClearEvents();
        }
        EventDisplaysFiltered.Clear();
        bool empty = string.IsNullOrEmpty(searchText);
        foreach (EventInfoDisplay eventInfo in EventDisplays)
        {
            if (empty || eventInfo.Matches(searchText))
            {
                EventDisplaysFiltered.Add(eventInfo);
            }
        }
    }

    public bool ShowEventById(string eventId)
    {
        if (LocationInfoCache.EventsCache.TryGetValue(eventId, out EventInfo? eventInfo))
        {
            ShowEvent(EventInfoDisplay.Make(eventInfo, NpcInfo.Name));
        }
        return true;
    }

    public bool ShowEvent(EventInfoDisplay eventInfo)
    {
        if (CurrentEventInfo != null)
            eventInfoStack.Push(CurrentEventInfo);
        CurrentEventInfo = eventInfo;
        return true;
    }

    public bool LeaveEvent()
    {
        if (CurrentEventInfo != null)
        {
            if (eventInfoStack.TryPop(out EventInfoDisplay? prevEvent))
                CurrentEventInfo = prevEvent;
            else
                CurrentEventInfo = null;
            return true;
        }
        return false;
    }

    internal void ClearEvents()
    {
        CurrentEventInfo = null;
        eventInfoStack.Clear();
    }
}

public sealed partial class GoalFriendsMadeContext(IGoalContext goalCtx)
    : AbstractPageListContext<FriendsMadeDisplay>(goalCtx, itemPerPageModifier: 6.0 / 8.0)
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

    public override string SearchText
    {
        get => field;
        set
        {
            if (!field.EqualsIgnoreCase(value))
            {
                field = value;
                filteredDisplay = null;
                OnPropertyChanged(new(nameof(SearchText)));
                if (selected == null)
                {
                    UpdateFilteredDisplayPaginated();
                }
                else
                {
                    selected.SearchEvents(field);
                }
            }
        }
    } = string.Empty;

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
    private FriendsMadeDisplay? selected = null;
    private string previousSearchText = string.Empty;
    public bool InEventPage => Selected != null;

    public void HandleLeftClick(FriendsMadeDisplay display)
    {
        if (display.ToggleReminder())
            return;
        Selected?.ClearEvents();
        Selected = display;
        previousSearchText = SearchText;
        if (string.IsNullOrEmpty(previousSearchText))
            display.SearchEvents(string.Empty);
        SearchText = string.Empty;
    }

    public override bool TryOpenPage()
    {
        MenuHandler.Handbook_SetDefaultFocusableTag(true);
        return base.TryOpenPage();
    }

    public override bool TryExitPage()
    {
        if (Selected != null)
        {
            if (Selected.LeaveEvent())
                return false;
            Selected = null;
            SearchText = previousSearchText;
            return false;
        }
        MenuHandler.Handbook_SetDefaultFocusableTag(false);
        return base.TryExitPage();
    }
}
