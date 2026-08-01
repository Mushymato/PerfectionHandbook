using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed partial class HandbookContext
{
    public static readonly Color ActiveColor = Color.White;
    public static readonly Color InactiveColor = Color.DimGray * 0.4f;
    public static readonly Color HiddenColor = Color.Black * 0.2f;
    public readonly Farmer who;

    private readonly PlayerOwned playerOwned = MenuHandler.IsPreloading
        ? new(new Dictionary<string, OwnedItemGroup>(), [])
        : ItemOwnedLookup.GetPlayerOwned();
    public readonly IReadOnlyList<GoalContext> PerfectionGoals;
    public readonly string PerfectionTitle;
    public readonly IReadOnlyList<GoalContext> AchievementGoals;
    public readonly IReadOnlyList<MiscContext> MiscPages;

    public HandbookContext(Farmer who)
    {
        this.who = who;

        this.PerfectionGoals = Goals.PerfectionGoals.Select(goal => GoalContext.Make(who, goal, playerOwned)).ToList();
        float perfectionPercent = PerfectionGoals.Sum(ctx =>
            ctx.Fulfillments[0].Percent * ((ctx.Goal as IPerfectionGoal)?.PercentWeight ?? 0f) / 100f
        );
        this.PerfectionTitle = I18n.Ui_Title_Perfection($"{perfectionPercent:P2}".Replace(" ", ""));

        this.AchievementGoals = Goals
            .AchievementGoals.Select(goal => GoalContext.Make(who, goal, playerOwned))
            .ToList();

        this.MiscPages =
        [
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
    }

    [Notify]
    private IGoalContext? selectedCtx = null;

    [Notify]
    public string exportMsg = I18n.Ui_Export();
    public string PageName => SelectedCtx?.PageName ?? "Main";

    public string PlayerName => who.displayName;
    public string FarmName =>
        Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", who.farmName.Value);
    public Texture2D FarmerPanel => DrawHelper.GetEntireFarmer(who) ?? Game1.daybg;
    public SDUISprite FarmIcon =>
        Game1.whichModFarm != null
            ? new(DrawHelper.SafeLoad(Game1.whichModFarm.IconTexture))
            : new(
                Game1.mouseCursors,
                new Rectangle(22 * (Game1.whichFarm % 5), 324 + 21 * (Game1.whichFarm / 5), 22, 20)
            );

    public void ChangePage(IGoalContext ctx)
    {
        if (ctx.PageCtx != null)
            SelectedCtx = ctx;
    }

    public void ExportCard()
    {
        string exportPath = Path.Combine(
            ModEntry.help.DirectoryPath,
            $"{who.displayName}-{who.farmName}-{Game1.CurrentSeasonDisplayName}-{Game1.dayOfMonth}.png"
        );
        try
        {
            MenuHandler.ExportCard(this, exportPath);
            ExportMsg = I18n.Ui_ExportPath(exportPath);
        }
        catch (Exception ex)
        {
            ModEntry.Log($"Failed to export card:\n{ex}", LogLevel.Warn);
            ExportMsg = I18n.Ui_ExportError();
        }
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
