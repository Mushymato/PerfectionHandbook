using PerfectionHandbook.GUI;
using PerfectionHandbook.Integration;
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
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    public static void ShowHandbook()
    {
        if (!Context.IsWorldReady)
            return;
        HandbookContext context = new(Game1.player);
        IMenuController menuCtrl = viewEngine.CreateMenuControllerFromAsset(VIEW_ASSET_HANDBOOK, context);
        menuCtrl.CloseAction = context.CloseAction;
        menuCtrl.EnableCloseButton();
        Game1.activeClickableMenu = menuCtrl.Menu;
    }
}
