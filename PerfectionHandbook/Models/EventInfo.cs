using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Characters;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.Models;

public enum EventLinkKind
{
    None,
    Event,
    Friend,
}

public sealed record EventLink(string Label, string? Link)
{
    public readonly Color TextColor = Link != null ? Color.Blue : Game1.textColor;
    public readonly Color TextHoverColor = Link != null ? Color.CornflowerBlue : Game1.textColor;
}

public sealed record EventPreconditionInfo(
    string Precond,
    bool Negated,
    string[] Args,
    EventPreconditionDelegate Handler,
    ArgGetInfo ArgInfo,
    EventLinkKind LinkKind,
    EventLink[]? Links
)
{
    public readonly string DisplayText = ArgInfo.FormArgDesc(Negated, Handler.Method.Name, Args);
    public readonly string PrecondText = ArgInfo.FormPrecondName(Negated, Handler.Method.Name);

    public bool Evaluate(EventInfo eventInfo)
    {
        if (LocationInfoCache.Cache.TryGetValue(eventInfo.LocationId, out LocationInfo? locationInfo))
        {
            return Handler(locationInfo.Location, eventInfo.EventId, Args) == !Negated;
        }
        return false;
    }

    internal static EventPreconditionDelegate? Make(string precond)
    {
        if (Event.TryGetPreconditionHandler(precond, out EventPreconditionDelegate handler))
        {
            return handler;
        }
        return null;
    }
}

public sealed record EventInfo(
    string EventId,
    EventPreconditionInfo[] Preconditions,
    string[] Commands,
    string[] Actors,
    string LocationId,
    string LocationName,
    string EventKey,
    IReadOnlyDictionary<string, int>? FriendshipReqs,
    IModNameInfo? ModNameInfo
)
{
    public readonly string HeaderText = $"{EventId} @ {LocationName}";

    public bool HasModName => ModNameInfo != null;
    public string ModName => ModNameInfo?.ModName ?? string.Empty;
    public Color ModNameTint => ModNameInfo?.ModNameColor ?? Game1.textColor;

    internal int GetRequiredFriendship(string forNPC)
    {
        if (FriendshipReqs?.TryGetValue(forNPC, out int points) ?? false)
            return points;
        return -1;
    }

    #region static setup
    internal static EventPreconditionDelegate? Precondition_Friendship =>
        field ??= EventPreconditionInfo.Make("Friendship");
    internal static EventPreconditionDelegate? Precondition_SawEvent =>
        field ??= EventPreconditionInfo.Make("SawEvent");
    internal static EventPreconditionDelegate? Precondition_NotSawEvent => field ??= EventPreconditionInfo.Make("k");
    internal static EventPreconditionDelegate? Precondition_ActiveDialogueEvent =>
        field ??= EventPreconditionInfo.Make("ActiveDialogueEvent");
    internal static EventPreconditionDelegate? Precondition_NotActiveDialogueEvent =>
        field ??= EventPreconditionInfo.Make("A");
    internal static Regex EventCTPattern = new(
        @"eventSeen_(.+)(?:_memory_oneday|_memory_oneweek|_memory_twoweeks|_memory_fourweeks|_memory_eightweeks|_memory_oneyear)?"
    );

    private static Dictionary<string, int>? GetFriendshipReqs(EventPreconditionInfo[] preconditions)
    {
        Dictionary<string, int>? friendshipReqs = [];
        foreach (EventPreconditionInfo precond in preconditions)
        {
            if (!precond.Negated && precond.Handler == Precondition_Friendship && precond.Args.Length >= 3)
            {
                for (int i = 2; i < precond.Args.Length; ++i)
                {
                    string npcName = precond.Args[i - 1];
                    if (int.TryParse(precond.Args[i], out int points))
                    {
                        friendshipReqs[npcName] = points;
                    }
                }
            }
        }
        return friendshipReqs.Any() ? friendshipReqs : null;
    }

    public static bool TryParse(
        string locationId,
        string locationName,
        string key,
        string script,
        IAssetName assetName,
        [NotNullWhen(true)] out EventInfo? info
    )
    {
        info = null;

        string[] idPrecond = Event.SplitPreconditions(key);
        string eventId = idPrecond[0];
        if (!TryNormalizePrecond(idPrecond.Skip(1), out EventPreconditionInfo[]? preconds))
        {
            return false;
        }
        string[] commands = Event.ParseCommands(script);
        List<string> actors = [];

        if (
            ArgUtility.TryGet(
                commands,
                2,
                out string setrawCharacterPositionsupChara,
                out _,
                allowBlank: false,
                "string rawCharacterPositions"
            )
        )
        {
            string[] array = ArgUtility.SplitBySpace(setrawCharacterPositionsupChara);
            for (int i = 0; i < array.Length; i += 4)
            {
                if (ArgUtility.TryGet(array, i, out string actorName, out _, allowBlank: true, "string actorName"))
                {
                    actors.Add(actorName);
                }
            }
        }

        info = new(
            eventId,
            preconds,
            commands,
            actors.ToArray(),
            locationId,
            locationName,
            key,
            GetFriendshipReqs(preconds),
            ModEntry.modNameAPI?.GetModName_FromAssetAndId(assetName, eventId)
        );
        return true;

        static bool TryNormalizePrecond(
            IEnumerable<string> preconds,
            [NotNullWhen(true)] out EventPreconditionInfo[]? normalized
        )
        {
            normalized = null;
            List<EventPreconditionInfo> normalizedList = [];
            foreach (string precond in preconds)
            {
                string[] parts = ArgUtility.SplitBySpaceQuoteAware(precond);
                string realPrecond = parts[0];
                bool negated = false;
                if (realPrecond.StartsWith('!'))
                {
                    realPrecond = realPrecond[1..];
                    negated = true;
                }
                if (!Event.TryGetPreconditionHandler(realPrecond, out EventPreconditionDelegate handler))
                {
                    return false;
                }
                (EventLinkKind, EventLink[])? links = GetLinks(parts, handler);
                normalizedList.Add(
                    new(
                        realPrecond,
                        negated,
                        parts,
                        handler,
                        DelegateInspector.ExtractTryGetPairs(handler),
                        links?.Item1 ?? EventLinkKind.None,
                        links?.Item2
                    )
                );
            }
            normalized = normalizedList.ToArray();
            return true;
        }
    }

    private static (EventLinkKind, EventLink[])? GetLinks(string[] parts, EventPreconditionDelegate handler)
    {
        if (handler == Precondition_SawEvent || handler == Precondition_NotSawEvent)
        {
            return (
                EventLinkKind.Event,
                parts
                    .Skip(1)
                    .Select(id => new EventLink(id, LocationInfoCache.EventsLUT.ContainsKey(id) ? id : null))
                    .ToArray()
            );
        }
        else if (handler == Precondition_ActiveDialogueEvent || handler == Precondition_NotActiveDialogueEvent)
        {
            List<EventLink> eventLinksList = [];
            foreach (string ctId in parts.Skip(1))
            {
                if (
                    EventCTPattern.Match(ctId) is Match match
                    && match.Success
                    && match.Groups[1].Value is string eventId
                    && LocationInfoCache.EventsLUT.ContainsKey(eventId)
                )
                {
                    eventLinksList.Add(new(ctId, match.Groups[1].Value));
                }
                else
                {
                    eventLinksList.Add(new(ctId, null));
                }
            }
            return (EventLinkKind.Event, eventLinksList.ToArray());
        }
        else if (handler == Precondition_Friendship)
        {
            List<EventLink> friendLinkList = [];
            for (int i = 2; i < parts.Length; ++i)
            {
                string npcName = parts[i - 1];
                if (
                    Game1.characterData.TryGetValue(npcName, out CharacterData? chara)
                    && NPCInfo.CheckCanEventuallySocialize(chara)
                )
                {
                    friendLinkList.Add(
                        new($"{TokenParser.ParseText(chara.DisplayName) ?? npcName} {parts[i]}", npcName)
                    );
                }
                else
                {
                    friendLinkList.Add(new($"{npcName} {parts[i]}", null));
                }
            }
            return (EventLinkKind.Friend, friendLinkList.ToArray());
        }
        return null;
    }
    #endregion
}

public sealed class EventDescriptionData
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? DescriptionSpoiler { get; set; }

    internal string? GetHeaderText(EventInfo info)
    {
        if (string.IsNullOrEmpty(DisplayName))
            return null;
        return $"{TokenParser.ParseText(DisplayName)} @ {info.LocationName}";
    }

    internal string? GetDescription(EventInfo info)
    {
        // always use the local player
        if ((Game1.player.eventsSeen.Contains(info.EventId) ? DescriptionSpoiler : Description) is not string desc)
            return null;
        return TokenParser.ParseText(desc);
    }
}
