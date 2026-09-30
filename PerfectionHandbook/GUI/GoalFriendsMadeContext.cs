using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed partial record FriendsMadeDisplay(NPCInfo NpcInfo, SDUISprite MugShotSprite)
    : EventHoldingDisplay(
        NpcInfo
            .Events.Values.Select(ei => EventInfoDisplay.Make(ei, NpcInfo.Name))
            .OrderBy(static eid =>
                (eid.HasRequiredFriendshipForNPC ? eid.RequiredFriendshipForNPC : int.MaxValue, eid.Info.EventId)
            )
            .ToList()
    ),
        IPageDisplayEntry
{
    public override int GetHashCode() => NpcInfo.Name.GetHashCode();

    [Notify]
    private Friendship? currentFriendship = null;

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

    private readonly int eventsHere = NpcInfo.Events.Count;

    [Notify]
    private int seenCount = 0;

    [Notify]
    private int readyCount = 0;

    public bool HasReadyEvents => ReadyCount > 0;
    public string EventCountText => I18n.Ui_Event_Count(SeenCount, eventsHere, ReadyCount);

    public string ScreenRead => $"{DisplayName} {FriendshipPointDisplay} {EventCountText}";
    public string FriendDetailText =>
        NpcInfo.Data.BirthSeason != null ? $"{NpcInfo.BirthdayText} | {EventCountText}" : EventCountText;
    public ReminderEntry? Reminder { get; } =
        MenuHandler.Reminders.GetOrCreateEntry(ReminderEntryFactory.Kind_FriendsMade, NpcInfo.Name);

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
        int seenCount = 0;
        int readyCount = 0;
        foreach (EventInfoDisplay eventDisp in EventDisplays)
        {
            eventDisp.HasSeen = who.eventsSeen.Contains(eventDisp.Info.EventId);
            if (eventDisp.HasSeen)
                seenCount++;
            if (eventDisp.EventStatus == EventInfoDisplay.EventInfoDisplayStatus.Ready)
                readyCount++;
        }
        SeenCount = seenCount;
        ReadyCount = readyCount;
    }

    public bool ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);
}

public sealed partial class GoalFriendsMadeContext(IGoalContext goalCtx)
    : AbstractPageListContext<FriendsMadeDisplay>(goalCtx, itemPerPageModifier: 9.0 / 13.0)
{
    public override bool HasSortModes => true;

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
            PageSortMode.Default => displayList
                .OrderByDescending(static disp =>
                    (disp.HasReadyEvents ? 1 : 0, disp.NpcInfo.CanEventuallySocialize ? 1 : 0, disp.FriendshipFill)
                )
                .ThenBy(static disp => disp.DisplayName, ModEntry.displayStringComparer)
                .ToList(),
            PageSortMode.Count => displayList
                .OrderByDescending(static disp => (disp.NpcInfo.CanEventuallySocialize ? 1 : 0, disp.FriendshipFill))
                .ToList(),
            PageSortMode.Name => displayList
                .OrderBy(static disp => disp.NpcInfo.DisplayName, ModEntry.displayStringComparer)
                .ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }

    [Notify]
    private FriendsMadeDisplay? selected = null;
    private string previousSearchText = string.Empty;
    public bool HasSelected => Selected != null;
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
        Game1.playSound("shiny4");
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
        InSubPage = true;
        Game1.playSound("shiny4");
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
            Game1.playSound("shiny4");
            Selected = null;
            SearchText = previousSearchText;
            InSubPage = false;
            debounceScrollProgress = true;
            return false;
        }
        MenuHandler.Handbook_SetDefaultFocusableTag(false);
        return base.TryExitPage();
    }
}
