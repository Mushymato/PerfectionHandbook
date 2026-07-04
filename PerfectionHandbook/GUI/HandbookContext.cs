using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed partial class HandbookContext(Farmer who)
{
    public static readonly Color ActiveColor = Color.White;
    public static readonly Color InactiveColor = Color.DimGray * 0.4f;
    public static readonly Color HiddenColor = Color.Black * 0.2f;

    private readonly PlayerOwned playerOwned = MenuHandler.IsPreloading
        ? new(new Dictionary<string, OwnedItemGroup>(), [])
        : ItemOwnedLookup.GetPlayerOwned();
    public IReadOnlyList<GoalContext> PerfectionGoals
    {
        get => field ??= Goals.PerfectionGoals.Select(goal => GoalContext.Make(who, goal, playerOwned)).ToList();
    } = null;
    public IReadOnlyList<GoalContext> AchievementGoals
    {
        get => field ??= Goals.AchievementGoals.Select(goal => GoalContext.Make(who, goal, playerOwned)).ToList();
    } = null;
    public IReadOnlyList<MiscContext> MiscPages
    {
        get =>
            field ??= [
                new MiscContext(
                    who,
                    playerOwned,
                    "Misc_Crop_Calendar",
                    I18n.Ui_Misc_CropCalendar(),
                    ItemRegistry.GetDataOrErrorItem("(O)889"),
                    string.Empty,
                    (ctx) => new GoalCropListContext(ctx, CropListKind.Any)
                ),
                new MiscContext(
                    who,
                    playerOwned,
                    "Misc_Required_Ingredients",
                    I18n.Ui_Misc_Ingredients(),
                    ItemRegistry.GetDataOrErrorItem("(O)419"),
                    string.Empty,
                    (ctx) => new GoalRecipesIngredientContext(ctx)
                ),
                new MiscContext(
                    who,
                    playerOwned,
                    "Misc_Mod_Config",
                    I18n.Ui_Misc_ModConfig(),
                    ItemRegistry.GetDataOrErrorItem("(O)112"),
                    string.Empty,
                    (ctx) => new ModConfigContext(ModEntry.config)
                ),
            ];
    } = null;

    [Notify]
    private IGoalContext? selectedCtx = null;
    public string PageName => SelectedCtx?.PageName ?? "Main";

    public void ChangePage(IGoalContext ctx)
    {
        if (ctx.PageCtx != null)
            SelectedCtx = ctx;
    }

    internal void CloseAction()
    {
        if (SelectedCtx != null)
        {
            SelectedCtx = null;
        }
        else
        {
            Game1.exitActiveMenu();
            Game1.player.forceCanMove();
        }
    }
}
