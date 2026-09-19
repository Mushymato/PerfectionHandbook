using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed partial record LocationDisplay(LocationInfo Info)
    : EventHoldingDisplay(Info.Events!.Values.Select(EventInfoDisplay.Make).ToList()),
        IPageDisplayEntry
{
    public readonly string DisplayName = Info.Location.DisplayName ?? Info.LocationId;
    public readonly string EventCount = I18n.Ui_Event_Count(Info.Events!.Count);
    public string ScreenRead => $"{DisplayName} {EventCount}";
    public bool Needed => true;

    public bool SearchMatch(string txt)
    {
        return DisplayName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who) { }

    public ReminderEntry? Reminder => throw new NotImplementedException();

    public static LocationDisplay Make(LocationInfo Info) => new(Info);
}

public sealed partial class GoalLocationContext(IGoalContext goalCtx)
    : AbstractPageListContext<LocationDisplay>(
        goalCtx,
        canToggleNeeded: false,
        canToggleCountMode: false,
        canSetReminders: false,
        itemPerPageModifier: 12.5 / 13.0
    )
{
    private string previousSearchText = string.Empty;

    [Notify]
    private LocationDisplay? selected = null;
    public bool HasSelected => Selected != null;

    public void HandleLeftClick(LocationDisplay display)
    {
        Selected?.ClearEvents();
        previousSearchText = SearchText;
        InSubPage = true;
        Selected = display;
        Selected.SearchEvents(string.Empty);
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
            Selected.ShowEventImpl(EventInfoDisplay.Make(eventInfo));
        }
        return true;
    }

    protected override IReadOnlyList<LocationDisplay> MakeAllDisplay()
    {
        List<LocationDisplay> locations = [];
        foreach (LocationInfo info in LocationInfoCache.Cache.Values)
        {
            if (!(info.Events?.Any() ?? false))
                continue;
            locations.Add(LocationDisplay.Make(info));
        }
        return locations;
    }

    public override bool TryExitPage()
    {
        if (Selected != null)
        {
            if (Selected.LeaveEvent())
                return false;
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
