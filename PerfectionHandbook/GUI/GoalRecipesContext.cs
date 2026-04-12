using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
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
        if (Recipe.isCookingRecipe)
        {
            learnt = who.cookingRecipes.ContainsKey(Recipe.name);
            completedCount = who.recipesCooked.GetValueOrDefault(Info.Datum.ItemId, 0);
        }
        else
        {
            if (who.craftingRecipes.TryGetValue(Recipe.name, out int crafted))
            {
                learnt = true;
                completedCount = crafted;
            }
            else
            {
                learnt = false;
                completedCount = 0;
            }
        }

        UpdateCount();
    }
}

public sealed class GoalRecipesContext : AbstractItemCountContext<RecipeDisplay>
{
    private readonly bool IsCooking;

    public GoalRecipesContext(GoalContext goalCtx, bool isCooking)
        : base(goalCtx)
    {
        IsCooking = isCooking;
        // switch to CountMode.Completed as default
        if (CanToggleCountMode)
            ClickToggleCount();
    }

    public override string CompleteCountToggleText => IsCooking ? I18n.Ui_CountingCooked() : I18n.Ui_CountingCrafted();

    protected override IReadOnlyList<RecipeDisplay> MakeAllDisplay()
    {
        List<RecipeDisplay> recipeDisplayList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            foreach (CraftingRecipe recipe in itemInfo.FromRecipe)
            {
                if (recipe.isCookingRecipe == IsCooking)
                {
                    int ownedCount = 0;
                    if (
                        GoalCtx.OwnedInfo.OwnedGroups.TryGetValue(
                            itemInfo.Datum.QualifiedItemId,
                            out OwnedItemGroup? group
                        )
                    )
                        ownedCount = group.CountRepr.ReprStack;
                    recipeDisplayList.Add(new(itemInfo, ownedCount, recipe, GoalCtx.OwnedInfo));
                }
            }
        }
        return recipeDisplayList;
    }
}
