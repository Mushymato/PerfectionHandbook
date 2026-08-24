global using SObject = StardewValley.Object;
using System.Diagnostics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;

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
    internal static ModConfig config = new();
    internal static Integration.IModNameAPI? modNameAPI = null;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        help = helper;
        try
        {
            config = help.ReadConfig<ModConfig>();
        }
        catch (Exception ex)
        {
            Log($"Read config error:\n{ex}");
            config = new ModConfig();
            help.WriteConfig(config);
        }

        ReminderEntryFactory.Setup();
        AssetManager.Setup();

        help.Events.GameLoop.GameLaunched += OnGameLaunched;
        help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        help.Events.GameLoop.DayStarted += OnDayStarted;
        help.Events.GameLoop.Saving += OnSaving;
        help.Events.Content.AssetsInvalidated += OnAssetInvalidated;
        help.Events.Content.LocaleChanged += OnLocaleChanged;
        help.Events.Input.ButtonsChanged += OnButtonsChanged;

        // help.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked_PreloadHandbook;

        help.ConsoleCommands.Add(
            "ph-show",
            "Debug show the handbook",
            static (cmd, args) => MenuHandler.ShowHandbook()
        );
        help.ConsoleCommands.Add(
            "ph-hud",
            "Debug toggle the reminder hud",
            static (cmd, args) => MenuHandler.Reminders.ToggleVisibility()
        );
        help.ConsoleCommands.Add(
            "ph-qicat",
            "Debug show the vanilla perfection tracker",
            static (cmd, args) => Game1.currentLocation?.ShowQiCat()
        );
        help.ConsoleCommands.Add(
            "ph-card",
            "Export handbook card png",
            static (cmd, args) =>
            {
                if (!Context.IsWorldReady)
                    return;
                MenuHandler.ExportCard(new(Game1.player), Path.Combine(help.DirectoryPath, "testcard.png"));
            }
        );
#if DEBUG
        help.ConsoleCommands.Add(
            "ph-invalidate",
            "Invalidate some asset",
            static (cmd, args) => help.GameContent.InvalidateCache(args[0])
        );
        help.ConsoleCommands.Add(
            "ph-tryget",
            "Test tryget extraction",
            static (cmd, args) =>
            {
                if (Event.TryGetPreconditionHandler("Tile", out EventPreconditionDelegate handler))
                {
                    ArgGetInfo argGetInfo = DelegateInspector.ExtractTryGetPairs(handler);
                    argGetInfo.LogRepr();
                    Log(argGetInfo.FormArgDesc(false, ["", "44", "55"]));
                }
            }
        );
#endif
    }

    /// <inheritdoc/>
    public override object? GetApi(IModInfo mod) => new PerfectionHandbookAPI(mod);

    private static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        InvalidateTracker.OnAssetInvalidated(e);
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        modNameAPI = help.ModRegistry.GetApi<Integration.IModNameAPI>("mushymato.ModNameTooltip");
        MenuHandler.Setup();
        ItemInfoCache.Setup();
        GoalSkillLeveledContext.Setup();
    }

    // private static void OneSecondUpdateTicked_PreloadHandbook(object? sender, OneSecondUpdateTickedEventArgs e)
    // {
    //     help.Events.GameLoop.OneSecondUpdateTicked -= OneSecondUpdateTicked_PreloadHandbook;
    //     MenuHandler.PreloadHandbook();
    // }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // preload the cache
        // DelayedAction.functionAfterDelay(() => ItemInfoCache.GetItemInfo(), 0);
        MenuHandler.PreloadHandbook();
        MenuHandler.Reminders.SaveLoaded(Game1.player);
    }

    private static void OnSaving(object? sender, SavingEventArgs e)
    {
        MenuHandler.Reminders.Saving(Game1.player);
    }

    private static void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        foreach ((_, RemindersHUD hud) in MenuHandler.reminders.GetActiveValues())
        {
            hud.Deactivate();
        }
        DrawHelper.DisposeCache();
        ItemInfoCache.ClearLocationCache();
    }

    private static void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
    {
        DrawHelper.DisposeCache();
    }

    private static void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (config.ShowHandbookKey.JustPressed())
        {
            MenuHandler.ShowHandbook();
        }
        else if (config.RemindersToggleKey.JustPressed())
        {
            MenuHandler.Reminders.ToggleVisibility();
        }
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        NPCInfoCache.RecheckNPCInstances();
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
