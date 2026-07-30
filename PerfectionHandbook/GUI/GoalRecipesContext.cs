using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(ItemInfo Info, int OwnedCount, CraftingRecipe Recipe, PlayerOwned OwnedInfo)
    : AbstractItemCountDisplay(Info, OwnedCount)
{
    public override Color DisplayTint
    {
        get
        {
            if (countMode == CountMode.Owned)
            {
                return base.DisplayTint;
            }
            else
            {
                return learnt
                    ? CanCraft
                        ? HandbookContext.ActiveColor
                        : HandbookContext.InactiveColor
                    : HandbookContext.HiddenColor;
            }
        }
    }
    public override bool Needed => completedCount == 0;
    public override SDUITooltipData Tooltip =>
        new(
            " ",
            Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
            CraftingRecipe: Recipe,
            AdditionalCraftingMaterials: OwnedInfo.OwnedRepr
        );

    public readonly bool CanCraft = Recipe.doesFarmerHaveIngredientsInInventory(OwnedInfo.OwnedRepr);
    public override bool HasCount => Recipe.numberProducedPerCraft > 1;
    private bool learnt;

    public override void SetStatus(Farmer who)
    {
        completedCount = Recipe.GetRecipeCraftedCount(Info, who);
        learnt = completedCount >= 0;
        completedCount = completedCount < 0 ? 0 : completedCount;
        UpdateCount();
    }

    public override ReminderEntry? Reminder =>
        field ??= new(
            Recipe.isCookingRecipe ? ReminderEntryFactory.Kind_CookingRecipe : ReminderEntryFactory.Kind_CraftingRecipe,
            Recipe.name
        );
}

public sealed class GoalRecipesContext(GoalContext goalCtx, bool isCooking)
    : AbstractItemCountContext<RecipeDisplay>(goalCtx, defaultCountMode: CountMode.Completed)
{
    private readonly bool IsCooking = isCooking;

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
                    int ownedCount = 0;
                    if (
                        GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(
                            itemInfo.Datum.QualifiedItemId,
                            out OwnedItemGroup? group
                        )
                    )
                        ownedCount = group.CountRepr.ReprStack;
                    recipeDisplayList.Add(new(itemInfo, ownedCount, recipe.Recipe, GoalCtx.OwnedInfo));
                }
            }
        }
        return recipeDisplayList;
    }
}
