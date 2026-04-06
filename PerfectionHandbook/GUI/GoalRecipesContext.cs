using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public partial record RecipeDisplay(CraftingRecipe Recipe, ItemInfo Info, PlayerOwned OwnedInfo) : IPageDisplayEntry
{
    [Notify]
    public Color displayTint = HandbookContext.ActiveColor;

    public readonly SDUITooltipData Tooltip = new(
        " ",
        Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
        CraftingRecipe: Recipe,
        AdditionalCraftingMaterials: OwnedInfo.OwnedRepr
    );

    public readonly bool CanCraft = Recipe.doesFarmerHaveIngredientsInInventory(OwnedInfo.OwnedRepr);
    public bool Learnt { get; private set; } = false;
    public bool Needed { get; private set; } = false;

    public void SetStatus(Farmer who)
    {
        if (Recipe.isCookingRecipe)
        {
            Learnt = who.cookingRecipes.ContainsKey(Recipe.name);
            Needed = !who.recipesCooked.ContainsKey(Info.Datum.ItemId);
        }
        else
        {
            if (who.craftingRecipes.TryGetValue(Recipe.name, out int crafted))
            {
                Learnt = true;
                Needed = crafted == 0;
            }
            else
            {
                Learnt = false;
                Needed = true;
            }
        }

        if (!Learnt)
            DisplayTint = HandbookContext.HiddenColor;
        else if (!CanCraft)
            DisplayTint = HandbookContext.InactiveColor;
        else
            DisplayTint = HandbookContext.ActiveColor;
    }
}

public sealed class GoalRecipesContext(GoalContext goalCtx, bool isCooking)
    : AbstractGoalPageListContext<RecipeDisplay>(goalCtx)
{
    public readonly bool IsCooking = isCooking;

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
