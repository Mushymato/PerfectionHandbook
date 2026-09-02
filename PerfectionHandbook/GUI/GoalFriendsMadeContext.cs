using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record EventPreconditionInfoDisplay(EventPreconditionInfo Info, bool Status, string ForNPC)
{
    public readonly EventLinkKind LinkKind = Info.LinkKind;
    public readonly EventLink[]? Links = Info
        .Links?.Select(link => new EventLink(
            link.Label,
            Info.LinkKind == EventLinkKind.Friend && ForNPC == link.Link ? null : link.Link
        ))
        .ToArray();
}

public sealed record EventActorLink(SDUISprite MugShotSprite, string Label, string? Link);

public sealed partial record EventInfoDisplay(
    EventInfo Info,
    EventDescriptionData? Desc,
    string ForNPC,
    int RequiredFriendshipForNPC
)
{
    public override int GetHashCode() => HashCode.Combine(Info.EventId, ForNPC);

    [Notify]
    private bool hasSeen = false;

    [Notify]
    private bool isExpanded = false;

    public readonly int RequiredHeartLevelForNPC = RequiredFriendshipForNPC / NPC.friendshipPointsPerHeartLevel;
    public readonly bool HasRequiredFriendshipForNPC = RequiredFriendshipForNPC > -1;
    public readonly bool HasDesc = Desc != null;
    public readonly string EventHeaderText = Desc?.GetHeaderText(Info) ?? Info.HeaderText;
    public readonly string? EventDescription = Desc?.GetDescription(Info);
    public bool HasEventDescription => EventDescription != null;
    public readonly EventPreconditionInfoDisplay[] Preconds = Info
        .Preconditions.Select(precond => new EventPreconditionInfoDisplay(precond, precond.Evaluate(Info), ForNPC))
        .ToArray();
    public readonly EventActorLink[] ActorLinks = GetActorLinks(Info, ForNPC);

    private static EventActorLink[] GetActorLinks(EventInfo info, string forNPC)
    {
        List<EventActorLink> actorLinks = [];
        foreach (string actorNameRaw in info.Actors)
        {
            string actorName = actorNameRaw.Trim('?');
            if (
                NPCInfoCache.Cache.TryGetValue(actorName, out NPCInfo? npcInfo)
                && npcInfo.CanEventuallySocialize
                && npcInfo.GetMugShot(2f) is SDUISprite mugshot
            )
            {
                actorLinks.Add(new(mugshot, npcInfo.DisplayName, actorName != forNPC ? actorName : null));
            }
        }
        return actorLinks.ToArray();
    }

    internal bool Matches(string searchText)
    {
        return Info.HeaderText.ContainsIgnoreCase(searchText);
    }

    internal static EventInfoDisplay Make(EventInfo Info, string ForNPC)
    {
        return new(Info, AssetManager.GetEventDesc(Info.EventId), ForNPC, Info.GetRequiredFriendship(ForNPC));
    }
}

public sealed partial record FriendsMadeDisplay(NPCInfo NpcInfo, SDUISprite MugShotSprite) : IPageDisplayEntry
{
    public override int GetHashCode() => NpcInfo.Name.GetHashCode();

    [Notify]
    private Friendship? currentFriendship = null;

    [Notify]
    private EventInfoDisplay? currentEventInfo = null;
    private readonly Stack<EventInfoDisplay> eventInfoStack = [];
    public bool HasCurrentEventInfo => CurrentEventInfo != null;

    public Color DisplayTint =>
        NpcInfo.CanEventuallySocialize && CurrentFriendship == null
            ? HandbookContext.InactiveColor
            : HandbookContext.ActiveColor;
    public bool Needed =>
        NpcInfo.CountForPerfection && (CurrentFriendship == null || CurrentFriendship.Points < NpcInfo.MaxPoints);
    public float FriendshipFill =>
        100f * MathF.Min(CurrentFriendship?.Points ?? 0, NpcInfo.MaxPoints) / NpcInfo.MaxPoints;
    public string FriendshipFillLayout => $"{FriendshipFill}% stretch";

    public int HeartLevel => (CurrentFriendship?.Points ?? 0) / NPC.friendshipPointsPerHeartLevel;
    public string FriendshipPointDisplay =>
        I18n.Ui_Fulfillment_Dipslay(CurrentFriendship?.Points ?? 0, NpcInfo.MaxPoints);

    public readonly string DisplayName = NpcInfo.DisplayName;
    public string ScreenRead => $"{DisplayName} {FriendshipPointDisplay}";
    public ReminderEntry? Reminder { get; } = new ReminderEntry(ReminderEntryFactory.Kind_FriendsMade, NpcInfo.Name);

    public readonly IReadOnlyList<EventInfoDisplay> EventDisplays = NpcInfo
        .Events.Values.Select(ei => EventInfoDisplay.Make(ei, NpcInfo.Name))
        .OrderBy(static eid =>
            (eid.HasRequiredFriendshipForNPC ? eid.RequiredFriendshipForNPC : int.MaxValue, eid.Info.EventId)
        )
        .ToList();

    public readonly ObservableCollection<EventInfoDisplay> EventDisplaysFiltered = [];
    public List<EventInfoDisplay> TheCurrentEventInfo => CurrentEventInfo != null ? [CurrentEventInfo] : [];

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

    internal bool ShowEventImpl(EventInfoDisplay eventInfo)
    {
        if (CurrentEventInfo != null)
        {
            eventInfoStack.Push(CurrentEventInfo);
        }
        CurrentEventInfo = eventInfo;
        return true;
    }

    public bool LeaveEvent()
    {
        if (CurrentEventInfo != null)
        {
            if (eventInfoStack.TryPop(out EventInfoDisplay? prevEvent))
            {
                CurrentEventInfo = prevEvent;
            }
            else
            {
                CurrentEventInfo = null;
            }
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
    protected override string[] ValidSortModes => [SORTMODE_DEFAULT, SORTMODE_NAME, SORTMODE_COUNT];
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
                    UpdateFilteredDisplayPaginated();
                else
                    selected.SearchEvents(field);
            }
        }
    } = string.Empty;

    protected override IReadOnlyList<FriendsMadeDisplay> MakeAllDisplay()
    {
        List<FriendsMadeDisplay> friendDisplay = [];
        foreach (NPCInfo npcInfo in NPCInfoCache.Cache.Values)
        {
            if (!npcInfo.CountForPerfection && !npcInfo.Events.Any())
                continue;
            if (npcInfo.GetMugShot() is not SDUISprite mugshotSprite)
                continue;
            FriendsMadeDisplay display = new(npcInfo, mugshotSprite);
            if (display.MugShotSprite != null)
            {
                friendDisplay.Add(display);
            }
        }
        return friendDisplay;
    }

    protected override List<FriendsMadeDisplay> SortAllDisplay(List<FriendsMadeDisplay> displayList)
    {
        return SortMode switch
        {
            SORTMODE_NAME => displayList
                .OrderBy(static disp => disp.NpcInfo.CanEventuallySocialize ? 0 : 1)
                .ThenBy(static disp => disp.DisplayName, ModEntry.displayStringComparer)
                .ToList(),
            SORTMODE_COUNT => displayList
                .OrderByDescending(static disp => (disp.FriendshipFill, disp.NpcInfo.CanEventuallySocialize ? 1 : 0))
                .ToList(),
            SORTMODE_DEFAULT => displayList.OrderBy(static disp => disp.NpcInfo.Name).ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }

    [Notify]
    private FriendsMadeDisplay? selected = null;
    private string previousSearchText = string.Empty;
    public bool InEventPage => Selected != null;
    private readonly Stack<FriendsMadeDisplay> friendStack = [];

    public void HandleLeftClick(FriendsMadeDisplay display)
    {
        if (display.ToggleReminder())
            return;
        previousSearchText = SearchText;
        Selected?.ClearEvents();
        ShowFriend(display);
    }

    public bool ShowFriendById(string? npcId)
    {
        if (npcId == null)
            return false;
        if (Selected?.NpcInfo.Name == npcId)
            return false;
        FriendsMadeDisplay? display = AllDisplay.FirstOrDefault(disp => disp.NpcInfo.Name == npcId);
        if (display != null)
        {
            if (Selected != null && friendStack.All(friend => friend != Selected))
            {
                friendStack.Push(Selected);
            }
            display.ClearEvents();
            ShowFriend(display);
        }
        return false;
    }

    public bool ShowEvent(EventInfoDisplay eventInfo)
    {
        return Selected?.ShowEventImpl(eventInfo) ?? false;
    }

    public bool ShowEventById(string? eventId)
    {
        if (eventId == null || Selected == null)
            return false;
        if (LocationInfoCache.EventsLUT.TryGetValue(eventId, out EventInfo? eventInfo))
        {
            Selected.ShowEventImpl(EventInfoDisplay.Make(eventInfo, Selected.NpcInfo.Name));
        }
        return true;
    }

    private void ShowFriend(FriendsMadeDisplay display)
    {
        Selected = display;
        // needed to make sure events get their first pass populate
        if (string.IsNullOrEmpty(SearchText))
            Selected.SearchEvents(string.Empty);
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
            if (friendStack.TryPop(out FriendsMadeDisplay? display))
            {
                ShowFriend(display);
                return false;
            }
            Selected = null;
            SearchText = previousSearchText;
            return false;
        }
        MenuHandler.Handbook_SetDefaultFocusableTag(false);
        return base.TryExitPage();
    }
}
