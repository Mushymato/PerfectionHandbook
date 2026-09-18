using System.Collections.ObjectModel;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI.Shared;

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

    [Notify]
    private string eventHeaderTextToggled = string.Empty;
    private bool? eventHeaderTextToggledState = null;

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

    private static EventActorLink[] GetActorLinks(EventInfo info, string forNPC, int limit = 18)
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
                if (actorLinks.Count == limit)
                    return actorLinks.ToArray();
            }
        }
        return actorLinks.ToArray();
    }

    public bool ToggleHeaderText()
    {
        if (!eventHeaderTextToggledState.HasValue)
            return false;
        eventHeaderTextToggledState = !eventHeaderTextToggledState;
        if (eventHeaderTextToggledState.Value)
            EventHeaderTextToggled = Info.HeaderText;
        else
            EventHeaderTextToggled = EventHeaderText;
        return true;
    }

    internal bool Matches(string searchText)
    {
        if (Desc != null)
        {
            return EventHeaderText.Contains(searchText)
                || Info.HeaderText.ContainsIgnoreCase(searchText)
                || (EventDescription?.Contains(searchText) ?? false);
        }
        return Info.HeaderText.ContainsIgnoreCase(searchText);
    }

    internal static EventInfoDisplay Make(EventInfo Info)
    {
        EventInfoDisplay display = new(Info, AssetManager.GetEventDesc(Info.EventId), string.Empty, -1);
        display.eventHeaderTextToggledState = display.EventHeaderText != Info.HeaderText ? false : null;
        display.EventHeaderTextToggled = display.EventHeaderText;
        return display;
    }

    internal static EventInfoDisplay Make(EventInfo Info, string ForNPC)
    {
        EventInfoDisplay display = new(
            Info,
            AssetManager.GetEventDesc(Info.EventId),
            ForNPC,
            Info.GetRequiredFriendship(ForNPC)
        );
        display.eventHeaderTextToggledState = display.EventHeaderText != Info.HeaderText ? false : null;
        display.EventHeaderTextToggled = display.EventHeaderText;
        return display;
    }
}

public abstract partial record EventHoldingDisplay(IReadOnlyList<EventInfoDisplay> EventDisplays)
{
    [Notify]
    private EventInfoDisplay? currentEventInfo = null;
    private readonly Stack<EventInfoDisplay> eventInfoStack = [];
    public bool HasCurrentEventInfo => CurrentEventInfo != null;

    public readonly ObservableCollection<EventInfoDisplay> EventDisplaysFiltered = [];

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
            Game1.playSound("shiny4");
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
