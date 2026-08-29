using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.Models;

public sealed record NPCInfo(string Name, CharacterData Data)
{
    public readonly bool CountForPerfection =
        Data.PerfectionScore && !GameStateQuery.IsImmutablyFalse(Data.CanSocialize);
    public readonly bool CanEventuallySocialize = CheckCanEventuallySocialize(Data);
    public readonly int MaxPoints = (Data.CanBeRomanced ? 8 : 10) * 250;
    public NPC? Chara { get; set; } = null;
    public Dictionary<string, EventInfo> Events { get; private set; } = GetEvents(Name, LocationInfoCache.Cache.Values);

    public string BirthdayText =>
        Data.BirthSeason != null
            ? Game1.content.LoadString(
                "Strings\\UI:BirthdayOrder",
                Data.BirthDay,
                Utility.getSeasonNameFromNumber((int)Data.BirthSeason)
            )
            : string.Empty;

    public IModNameInfo? ModNameInfo { get; private set; } = ModEntry.modNameAPI?.GetModName_FromNpcName(Name);
    public bool HasModName => ModNameInfo != null;
    public string ModName => ModNameInfo?.ModName ?? string.Empty;
    public Color ModNameTint => ModNameInfo?.ModNameColor ?? Game1.textColor;
    public string DisplayName => Chara?.displayName ?? TokenParser.ParseText(Data.DisplayName);

    public SDUISprite? GetMugShot(float scale = 4f) => GetMugShot(Name, Data, Chara, scale);

    public static SDUISprite? GetMugShot(string name, CharacterData data, NPC? chara, float scale)
    {
        if (chara == null)
        {
            string textureName = "Characters\\" + NPC.getTextureNameForCharacter(name);
            if (Game1.content.DoesAssetExist<Texture2D>(textureName))
            {
                return new(
                    DrawHelper.SafeLoad(textureName),
                    data.MugShotSourceRect ?? new Rectangle(0, (data.Age == NpcAge.Child) ? 4 : 0, 16, 24),
                    FixedEdges: SDUIEdges.NONE,
                    SliceSettings: new(Scale: scale)
                );
            }
            return null;
        }
        return new(
            chara.Sprite.Texture,
            chara.getMugShotSourceRect(),
            FixedEdges: SDUIEdges.NONE,
            SliceSettings: new(Scale: scale)
        );
    }

    private static Dictionary<string, EventInfo> GetEvents(string name, IEnumerable<LocationInfo> locationInfos)
    {
        Dictionary<string, EventInfo> events = [];
        foreach (LocationInfo locInfo in locationInfos)
        {
            if (locInfo.Events == null)
                continue;
            foreach (EventInfo eventInfo in locInfo.Events.Values)
            {
                if (eventInfo.Actors.Contains(name))
                {
                    events[eventInfo.EventId] = eventInfo;
                }
            }
        }
        return events;
    }

    public void RefreshEvents(IEnumerable<LocationInfo> refreshedLocationInfo)
    {
        Events = GetEvents(Name, refreshedLocationInfo);
    }

    public static bool CheckCanEventuallySocialize(CharacterData data) =>
        !GameStateQuery.IsImmutablyFalse(data.CanSocialize);
}

public static class NPCInfoCache
{
    private static readonly HashTracker hashCharacters = new(
        nameof(Game1.characterData),
        static () => Game1.characterData.GetHashCode()
    );

    private static Dictionary<string, NPCInfo>? cache = null;
    public static IReadOnlyDictionary<string, NPCInfo> Cache => GetNPCInfo();

    internal static IReadOnlyDictionary<string, NPCInfo> GetNPCInfo()
    {
        Stopwatch? stopwatch = null;

        Dictionary<string, NPCInfo> cacheRet;

        // bool useCached = false;
        if (hashCharacters.CheckChanged() || cache == null)
        {
            stopwatch = Stopwatch.StartNew();
            cacheRet = cache = RefreshCache();
            RecheckNPCInstances();
        }
        else
        {
            cacheRet = cache;
            // useCached = true;
        }

        if (stopwatch != null)
            ModEntry.Log($"NPCInfoCache({Game1.ticks}): refreshed in {stopwatch.Elapsed}", LogLevel.Debug);
        return cacheRet;
    }

    private static Dictionary<string, NPCInfo> RefreshCache()
    {
        Dictionary<string, NPCInfo> cacheRet = [];
        foreach ((string key, CharacterData data) in Game1.characterData)
        {
            if (!cacheRet.ContainsKey(key))
            {
                cacheRet[key] = new(key, data);
            }
        }

        return cacheRet;
    }

    public static void RecheckNPCInstances()
    {
        if (cache == null || !Context.IsWorldReady)
            return;
        Utility.ForEachVillager(chara =>
        {
            if (!cache.TryGetValue(chara.Name, out NPCInfo? npcInfo))
                return true;
            npcInfo.Chara = chara;
            return true;
        });
    }

    public static void RefreshEvents(IEnumerable<LocationInfo> refreshedLocationInfo)
    {
        if (cache == null)
            return;
        foreach (NPCInfo npcInfo in cache.Values)
        {
            npcInfo.RefreshEvents(refreshedLocationInfo);
        }
    }
}
