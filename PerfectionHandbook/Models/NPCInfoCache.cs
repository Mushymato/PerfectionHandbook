using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;

namespace PerfectionHandbook.Models;

public sealed record NPCInfo(string Name, CharacterData Data)
{
    public readonly bool CountForPerfection =
        Data.PerfectionScore && !GameStateQuery.IsImmutablyFalse(Data.CanSocialize);
    public readonly int MaxPoints = (Data.CanBeRomanced ? 8 : 10) * 250;
    public NPC? Chara { get; set; } = null;

    public SDUISprite? GetMugShot()
    {
        if (Chara == null)
        {
            string textureName = "Characters\\" + NPC.getTextureNameForCharacter(Name);
            if (Game1.content.DoesAssetExist<Texture2D>(textureName))
            {
                return new(
                    DrawHelper.SafeLoad(textureName),
                    Data.MugShotSourceRect ?? new Rectangle(0, (Data.Age == NpcAge.Child) ? 4 : 0, 16, 24)
                );
            }
            return null;
        }
        return new(Chara.Sprite.Texture, Chara.getMugShotSourceRect());
    }
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
}
