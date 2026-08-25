using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace PerfectionHandbook;

public sealed class HandbookRefs
{
    public WeakReference<IMenuController?> ctrl = new(null);
    public WeakReference<HandbookContext?> ctx = new(null);

    public void SetTarget(IMenuController? ctrl, HandbookContext? ctx)
    {
        this.ctrl.SetTarget(ctrl);
        this.ctx.SetTarget(ctx);
    }
}

public static class MenuHandler
{
    private static IViewEngine viewEngine = null!;
    internal const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    internal const string VIEW_ASSET_HANDBOOK = $"{VIEW_ASSET_PREFIX}/handbook";
    internal const string VIEW_ASSET_REMINDERS = $"{VIEW_ASSET_PREFIX}/reminder-hud";
    internal const string VIEW_ASSET_CARD = $"{VIEW_ASSET_PREFIX}/card";
    internal static readonly string exportDir = Path.Combine(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"),
        ".smapi",
        ModEntry.ModId
    );

    internal static readonly PerScreen<RemindersHUD> reminders = new(() =>
        new(static (ctx) => viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_REMINDERS, ctx))
    );
    internal static RemindersHUD Reminders => reminders.Value;
    internal static readonly PerScreen<HandbookRefs> handbook = new(static () => new());

    public static void Setup()
    {
        Directory.CreateDirectory(exportDir);
        viewEngine = ModEntry.help.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
        viewEngine.PreloadAssets();
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif

        if (
            ModEntry.help.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons") is IIconicFrameworkApi iconic
        )
        {
            iconic.AddToolbarIcon(
                ModEntry.ModId,
                "LooseSprites/emojis",
                new(27, 54, 9, 9),
                I18n.Ui_Mod_Name,
                I18n.Ui_Mod_Desc,
                ShowHandbook,
                Reminders.ToggleVisibility
            );
        }
    }

    public static void ShowHandbook()
    {
        if (!Context.IsWorldReady)
            return;
        HandbookContext context = new(Game1.player);
        IMenuController? menuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_HANDBOOK, context);
        menuCtrl.CloseAction = HandbookCloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
        handbook.Value.SetTarget(menuCtrl, context);
    }

    private static void HandbookCloseAction()
    {
        HandbookRefs hbref = handbook.Value;
        if (hbref.ctx.TryGetTarget(out HandbookContext? ctx) && ctx != null && ctx.CloseAction())
        {
            if (hbref.ctrl.TryGetTarget(out IMenuController? ctrl))
            {
                ctrl.Dispose();
                hbref.SetTarget(null, null);
            }
        }
    }

    public static bool Handbook_FocusOnTaggedView(string name)
    {
        if (!Game1.options.gamepadControls)
        {
            return false;
        }
        if (handbook.Value.ctrl.TryGetTarget(out IMenuController? ctrl))
        {
            return ctrl.FocusOnTaggedView(name);
        }
        return false;
    }

    public static string ExportCard(HandbookContext context)
    {
        Farmer who = context.who;
        string exportFile;
        if (Context.IsMultiplayer)
        {
            int idx = 0;
            foreach ((int i, long uid) in who.team.cellarAssignments.Pairs)
            {
                if (who.UniqueMultiplayerID == uid)
                {
                    idx = i;
                    break;
                }
            }
            exportFile =
                $"{who.farmName}-{idx}-{who.displayName}-{Game1.CurrentSeasonDisplayName}-{Game1.dayOfMonth}.png";
        }
        else
        {
            exportFile = $"{who.farmName}-{who.displayName}-{Game1.CurrentSeasonDisplayName}-{Game1.dayOfMonth}.png";
        }
        exportFile = string.Join("_", exportFile.Split(Path.GetInvalidFileNameChars()));
        string exportPath = Path.Combine(exportDir, exportFile);
        IViewDrawable? drawable = viewEngine.CreateDrawableFromAsset(VIEW_ASSET_CARD);
        drawable.Context = context;
        drawable.MaxSize = new Vector2(1280, 4096);
        RenderTarget2D exportRT = DrawHelper.RenderDrawableToTarget(drawable);
        using Stream stream = File.Create(exportPath);
        exportRT.SaveAsPng(stream, exportRT.Width, exportRT.Height);
        exportRT.Dispose();
        return exportFile;
    }

    public static bool IsPreloading { get; private set; } = false;

    public static void PreloadHandbook()
    {
        if (Context.IsSplitScreen && !Context.IsMainPlayer)
            return;
        var _ = ItemInfoCache.Cache;
        IsPreloading = true;
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            HandbookContext context = new(Game1.player);
            IMenuController? menuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_HANDBOOK, context);
            menuCtrl.HideHUD = false;
            menuCtrl.OpenSound = string.Empty;
            GameTime gameTime = new();
            TimeSpan oneTick = TimeSpan.FromTicks(1);
            menuCtrl.Menu.update(gameTime);
            foreach (GoalContext ctx in context.PerfectionGoals)
                PreloadUpdatePage(context, menuCtrl, gameTime, oneTick, ctx);
            foreach (GoalContext ctx in context.AchievementGoals)
                PreloadUpdatePage(context, menuCtrl, gameTime, oneTick, ctx);
            foreach (MiscContext ctx in context.MiscPages)
                PreloadUpdatePage(context, menuCtrl, gameTime, oneTick, ctx);
            menuCtrl.Dispose();
            ModEntry.Log($"PreloadHandbook {stopwatch.Elapsed}");
        }
        finally
        {
            IsPreloading = false;
        }
    }

    private static void PreloadUpdatePage(
        HandbookContext context,
        IMenuController menuCtrl,
        GameTime gameTime,
        TimeSpan oneTick,
        IGoalContext ctx
    )
    {
        if (ctx.PageCtx == null)
            return;
        context.SelectedCtx = ctx;
        menuCtrl.Menu.update(gameTime);
        gameTime.TotalGameTime.Add(oneTick);
    }
}
