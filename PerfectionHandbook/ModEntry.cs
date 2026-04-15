global using SObject = StardewValley.Object;
using System.Diagnostics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerfectionHandbook;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.PerfectionHandbook";
    private static IMonitor mon = null!;
    internal static IModHelper help = null!;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        help = helper;

        help.Events.GameLoop.GameLaunched += OnGameLaunched;
        help.Events.GameLoop.UpdateTicked += OnUpdateTicked_PreloadHandbook;
        help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        help.Events.Content.AssetsInvalidated += OnAssetInvalidated;
        help.Events.Content.LocaleChanged += OnLocaleChanged;

        help.ConsoleCommands.Add(
            "ph-show",
            "Debug show the handbook",
            static (cmd, args) => MenuHandler.ShowHandbook()
        );
#if DEBUG
        help.ConsoleCommands.Add(
            "ph-invalidate",
            "Invalidate some asset",
            static (cmd, args) => help.GameContent.InvalidateCache(args[0])
        );
#endif
    }

    private static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        InvalidateTracker.OnAssetInvalidated(e);
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        MenuHandler.Register();
        ItemInfoCache.Setup();
        GoalSkillLeveledContext.Setup();
    }

    private void OnUpdateTicked_PreloadHandbook(object? sender, UpdateTickedEventArgs e)
    {
        if (Game1.ticks > 2)
        {
            MenuHandler.PreloadHandbook();
            help.Events.GameLoop.UpdateTicked -= OnUpdateTicked_PreloadHandbook;
        }
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // preload the cache
        DelayedAction.functionAfterDelay(() => ItemInfoCache.GetItemInfo(), 0);
    }

    private static void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        DrawHelper.DisposeCache();
        ItemInfoCache.ClearLocationCache();
    }

    private static void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
    {
        DrawHelper.DisposeCache();
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon.Log(msg, level);
    }
}
