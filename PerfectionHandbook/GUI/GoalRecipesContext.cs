using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(ItemInfo Info, CraftingRecipe Recipe, bool Excluded, PlayerOwned OwnedInfo)
    : AbstractItemCountDisplay(Info, 0)
{
    public override Color DisplayTint
    {
        get
        {
            if (countMode == CountMode.Owned)
            {
                return learnt
                    ? CanCraft
                        ? HandbookContext.ActiveColor
                        : HandbookContext.InactiveColor
                    : HandbookContext.HiddenColor;
            }
            else
            {
                return base.DisplayTint;
            }
        }
    }
    public override bool Needed => completedCount == 0 && !Excluded;
    public override SDUITooltipData Tooltip =>
        new(
            " ",
            Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
            Item: Info.ReprItem,
            CraftingRecipe: Recipe,
            AdditionalCraftingMaterials: OwnedInfo.OwnedRepr
        );

    public readonly bool CanCraft = Recipe.doesFarmerHaveIngredientsInInventory(OwnedInfo.OwnedRepr);
    private bool learnt;

    public override void SetStatus(Farmer who)
    {
        completedCount = Recipe.GetRecipeCraftedCount(Info, who);
        learnt = completedCount >= 0;
        completedCount = completedCount < 0 ? 0 : completedCount;
        UpdateCount();
    }

    public override ReminderEntry? Reminder { get; } =
        MenuHandler.Reminders.GetOrCreateEntry(
            Recipe.isCookingRecipe ? ReminderEntryFactory.Kind_CookingRecipe : ReminderEntryFactory.Kind_CraftingRecipe,
            Recipe.name
        );
}

public sealed class GoalRecipesContext(GoalContext goalCtx, bool isCooking)
    : AbstractItemCountContext<RecipeDisplay>(goalCtx, defaultCountMode: CountMode.Owned)
{
    private readonly bool IsCooking = isCooking;

    public override string OwnedCountToggleText => I18n.Ui_CountingReady();
    public override string CompleteCountToggleText => IsCooking ? I18n.Ui_CountingCooked() : I18n.Ui_CountingCrafted();

    protected override IReadOnlyList<RecipeDisplay> MakeAllDisplay()
    {
        List<RecipeDisplay> recipeDisplayList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            foreach (CraftingRecipeWithNeeds recipe in itemInfo.FromRecipe)
            {
                if (recipe.Recipe.isCookingRecipe == IsCooking)
                {
                    recipeDisplayList.Add(new(itemInfo, recipe.Recipe, recipe.Excluded, GoalCtx.OwnedInfo));
                }
            }
        }
        return recipeDisplayList;
    }
}
