global using SObject = StardewValley.Object;
using System.Diagnostics;
using System.Text;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
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
    internal static ModConfig config = new();
    internal static Integration.IModNameAPI? modNameAPI = null;
    internal static StringComparer displayStringComparer = StringComparer.CurrentCulture;

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

        help.Events.GameLoop.GameLaunched += OnGameLaunched;
        help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        help.Events.GameLoop.DayStarted += OnDayStarted;
        help.Events.GameLoop.Saving += OnSaving;
        help.Events.Content.LocaleChanged += OnLocaleChanged;
        help.Events.Input.ButtonsChanged += OnButtonsChanged;
        help.Events.Content.AssetRequested += OnAssetRequested;
        help.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
        displayStringComparer = StringComparer.Create(
            new(string.IsNullOrEmpty(help.Translation.Locale) ? "en-US" : help.Translation.Locale),
            false
        );

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
                MenuHandler.ExportCard(new(Game1.player));
            }
        );
        help.ConsoleCommands.Add(
            "ph-listevents",
            "List all events ids from the cache",
            static (cmd, args) =>
            {
                if (!Context.IsWorldReady)
                    return;
                StringBuilder sb = new("\n");
                foreach (string eventid in LocationInfoCache.EventsLUT.Keys.OrderBy(kv => kv))
                {
                    sb.AppendLine(eventid);
                }
                Log(sb.ToString());
            }
        );
#if DEBUG
        help.ConsoleCommands.Add(
            "ph-invalidate",
            "Invalidate some asset",
            static (cmd, args) => help.GameContent.InvalidateCache(args[0])
        );
#endif
    }

    /// <inheritdoc/>
    public override object? GetApi(IModInfo mod) => new PerfectionHandbookAPI(mod);

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        InvalidateTracker.OnAssetInvalidated(e);
        AssetManager.OnAssetsInvalidated(e);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        AssetManager.OnAssetRequested(e);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        modNameAPI = help.ModRegistry.GetApi<Integration.IModNameAPI>("mushymato.ModNameTooltip");
        MenuHandler.Setup();
        ItemInfoCache.Setup();
        GoalSkillLeveledContext.Setup();
        config.Register(ModManifest);
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // preload the cache
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
        LocationInfoCache.ClearCache();
        NPCInfoCache.ClearCache();
    }

    private static void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
    {
        DrawHelper.DisposeCache();
        InvalidateTracker.OnLocaleChanged();
        displayStringComparer = StringComparer.Create(
            new(string.IsNullOrEmpty(help.Translation.Locale) ? "en-US" : help.Translation.Locale),
            false
        );
    }

    private static void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu != null)
        {
            return;
        }
        if (
            config.ShowHandbookKey.JustPressed()
            || Game1.player.ActiveItem?.QualifiedItemId == AssetManager.ObjectQId_Book
                && Game1.didPlayerJustRightClick()
        )
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
        if (config.AutoExportCardPeriod > 0 && (Game1.Date.TotalDays + 1) % config.AutoExportCardPeriod == 0)
        {
            try
            {
                string exportFile = MenuHandler.ExportCard(new(Game1.player));
                Log($"Auto-exported card '{exportFile}'", LogLevel.Info);
                Game1.addHUDMessage(new(I18n.Ui_Export_Auto(exportFile), HUDMessage.screenshot_type));
            }
            catch (Exception ex)
            {
                Log($"Failed to auto-export card, disabling:\n{ex}", LogLevel.Warn);
                Game1.addHUDMessage(new(I18n.Ui_ExportError(I18n.Ui_Export())));
                config.AutoExportCardPeriod = 0;
                help.WriteConfig(config);
            }
        }
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
