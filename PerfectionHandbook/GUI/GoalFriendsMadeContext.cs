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

public sealed partial record EventInfoDisplay(EventInfo Info)
{
    [Notify]
    private bool hasSeen = false;

    [Notify]
    private bool isExpanded = false;

    internal bool Matches(string searchText)
    {
        return Info.HeaderText.ContainsIgnoreCase(searchText);
    }
}

public sealed partial record FriendsMadeDisplay(NPCInfo NpcInfo) : IPageDisplayEntry
{
    [Notify]
    private Friendship? currentFriendship = null;
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
        .Events.Values.OrderBy(ei => ei.LocationName)
        .Select(ei => new EventInfoDisplay(ei))
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
}

public sealed partial class GoalFriendsMadeContext(IGoalContext goalCtx)
    : AbstractPageListContext<FriendsMadeDisplay>(goalCtx)
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
                    OnPropertyChanged(new(nameof(FilteredDisplayPaginated)));
                }
                else
                {
                    selected.SearchEvents(field);
                }
            }
        }
    } = string.Empty;

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
    private FriendsMadeDisplay? selected = null;
    private string previousSearchText = string.Empty;
    public bool InEventPage => Selected != null;

    public void HandleLeftClick(FriendsMadeDisplay display)
    {
        if (display.ToggleReminder())
            return;
        Selected = display;
        previousSearchText = SearchText;
        if (string.IsNullOrEmpty(previousSearchText))
            display.SearchEvents(string.Empty);
        SearchText = string.Empty;
    }

    public override bool TryExitPage()
    {
        if (Selected != null)
        {
            Selected = null;
            SearchText = previousSearchText;
            return false;
        }
        return base.TryExitPage();
    }
}
