using System.Diagnostics;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewModdingAPI;
using StardewValley;

namespace PerfectionHandbook;

public static class MenuHandler
{
    private static IViewEngine viewEngine = null!;
    internal const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    internal const string VIEW_ASSET_HANDBOOK = $"{VIEW_ASSET_PREFIX}/handbook";

    public static void Register()
    {
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
                ShowHandbook
            );
        }
    }

    public static void ShowHandbook()
    {
        if (!Context.IsWorldReady)
            return;
        HandbookContext context = new(Game1.player);
        IMenuController? menuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_HANDBOOK, context);
        menuCtrl.CloseAction = context.CloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
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
            ModEntry.Log($"PreloadHandbook {stopwatch.Elapsed}", LogLevel.Info);
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
        Stopwatch stopwatch = Stopwatch.StartNew();
        context.SelectedCtx = ctx;
        menuCtrl.Menu.update(gameTime);
        gameTime.TotalGameTime.Add(oneTick);
        ModEntry.Log($"PreloadUpdatePage({ctx.PageName}) {stopwatch.Elapsed}", LogLevel.Info);
    }
}
