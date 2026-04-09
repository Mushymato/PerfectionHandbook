using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(CraftingRecipe Recipe, ItemInfo Info, PlayerOwned OwnedInfo)
    : AbstractItemCountDisplay(Info, null)
{
    public override SDUITooltipData Tooltip =>
        new(
            " ",
            Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
            CraftingRecipe: Recipe,
            AdditionalCraftingMaterials: OwnedInfo.OwnedRepr
        );

    public readonly bool CanCraft = Recipe.doesFarmerHaveIngredientsInInventory(OwnedInfo.OwnedRepr);
    public override bool HasCount => Recipe.numberProducedPerCraft > 1;

    public override void SetStatus(Farmer who)
    {
        Count = Recipe.numberProducedPerCraft;
        bool learnt;
        if (Recipe.isCookingRecipe)
        {
            learnt = who.cookingRecipes.ContainsKey(Recipe.name);
            Needed = !who.recipesCooked.ContainsKey(Info.Datum.ItemId);
        }
        else
        {
            if (who.craftingRecipes.TryGetValue(Recipe.name, out int crafted))
            {
                learnt = true;
                Needed = crafted == 0;
            }
            else
            {
                learnt = false;
                Needed = true;
            }
        }

        if (!learnt)
            DisplayTint = HandbookContext.HiddenColor;
        else if (!CanCraft)
            DisplayTint = HandbookContext.InactiveColor;
        else
            DisplayTint = HandbookContext.ActiveColor;
    }
}

public sealed class GoalRecipesContext(GoalContext goalCtx, bool isCooking)
    : AbstractItemCountContext<RecipeDisplay>(goalCtx)
{
    private readonly bool IsCooking = isCooking;

    protected override IReadOnlyList<RecipeDisplay> MakeAllDisplay()
    {
        List<RecipeDisplay> recipeDisplayList = [];
        foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
        {
            foreach (CraftingRecipe recipe in itemInfo.FromRecipe)
            {
                if (recipe.isCookingRecipe == IsCooking)
                {
                    recipeDisplayList.Add(new(recipe, itemInfo, GoalCtx.OwnedInfo));
                }
            }
        }
        return recipeDisplayList;
    }
}
