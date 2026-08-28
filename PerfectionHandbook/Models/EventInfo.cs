using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace PerfectionHandbook.Models;

public sealed record EventPreconditionInfo(
    string Precond,
    bool Negated,
    string[] Args,
    EventPreconditionDelegate Handler
)
{
    internal readonly ArgGetInfo ArgGetInfo = DelegateInspector.ExtractTryGetPairs(Handler);
    public string DisplayText => ArgGetInfo.FormArgDesc(Negated, Handler.Method.Name, Args);

    public bool Evaluate(EventInfo eventInfo)
    {
        if (LocationInfoCache.Cache.TryGetValue(eventInfo.LocationId, out LocationInfo? locationInfo))
        {
            return Handler(locationInfo.Location, eventInfo.EventId, Args) == !Negated;
        }
        return false;
    }

    internal static EventPreconditionDelegate? GetPrecondition(string precond)
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
    IModNameInfo? ModNameInfo,
    IReadOnlyDictionary<string, int>? FriendshipReqs
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
    private static EventPreconditionDelegate? Precondition_Friendship =>
        field ??= EventPreconditionInfo.GetPrecondition("Friendship");
    private static EventPreconditionDelegate? Precondition_SawEvent =>
        field ??= EventPreconditionInfo.GetPrecondition("SawEvent");
    private static EventPreconditionDelegate? Precondition_NotSawEvent =>
        field ??= EventPreconditionInfo.GetPrecondition("k");

    private static void ExtractPrecondInfo(
        EventPreconditionInfo[] preconditions,
        out Dictionary<string, int>? friendshipReqs
    )
    {
        friendshipReqs = [];
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
        friendshipReqs = friendshipReqs.Any() ? friendshipReqs : null;
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
                if (!ArgUtility.TryGet(array, i, out string actorName, out _, allowBlank: true, "string actorName"))
                {
                    continue;
                }
                actors.Add(actorName);
            }
        }

        ExtractPrecondInfo(preconds, out Dictionary<string, int>? friendshipReqs);

        info = new(
            eventId,
            preconds,
            commands,
            actors.ToArray(),
            locationId,
            locationName,
            key,
            ModEntry.modNameAPI?.GetModName_FromAssetAndId(assetName, eventId),
            friendshipReqs
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
                normalizedList.Add(new(realPrecond, negated, parts, handler));
            }
            normalized = normalizedList.ToArray();
            return true;
        }
    }
    #endregion
}
