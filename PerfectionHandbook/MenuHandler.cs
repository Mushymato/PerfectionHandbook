using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI;
using PerfectionHandbook.Integration;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

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
                $"{ModEntry.ModId}/AnimalManage",
                "LooseSprites/emojis",
                new(0, 126, 9, 9),
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
}
