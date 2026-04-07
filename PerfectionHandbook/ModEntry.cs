global using SObject = StardewValley.Object;
using System.Diagnostics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

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
        help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

        help.ConsoleCommands.Add(
            "ph-show",
            "Debug show the handbook",
            static (cmd, args) => MenuHandler.ShowHandbook()
        );
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        MenuHandler.Register();
        ItemInfoCache.Setup();
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // preload the cache
        var _ = ItemInfoCache.Cache;
        DrawHelper.PreloadCache();
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
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
