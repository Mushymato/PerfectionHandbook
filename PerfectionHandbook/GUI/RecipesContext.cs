using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(
    CraftingRecipe Recipe,
    ItemInfo Info,
    bool NotLearnt,
    IList<Item> AdditionalCraftingMaterials
)
{
    public readonly Color NotLearntTint = (NotLearnt ? 0.5f : 1f) * Color.White;
    public SDUITooltipData Tooltip = new(
        " ",
        Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
        CraftingRecipe: Recipe,
        AdditionalCraftingMaterials: AdditionalCraftingMaterials
    );
}

public abstract partial record RecipesContext(GoalContext GoalContext, bool IsCooking) : IGoalPageContext
{
    [Notify]
    public string searchText = string.Empty;

    [Notify]
    private Farmer? displayingFarmer = GoalContext.MyFulfillment.Who;

    [Notify]
    private bool showNeedToCraft = true;

    public void ClickBestFulfilment() { }

    public void ClickMyFulfilment() { }

    public IReadOnlyList<RecipeDisplay> FilteredRecipes
    {
        get
        {
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            List<RecipeDisplay> filteredItems = [];
            bool showNeedToCraft = ShowNeedToCraft;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                filteredItems.AddRange(GetRecipeDisplay(who, itemInfo, showNeedToCraft));
            }
            return filteredItems;
        }
    }

    public abstract IEnumerable<RecipeDisplay> GetRecipeDisplay(Farmer who, ItemInfo itemInfo, bool showNeed);
}

public sealed record CookingRecipesContext(GoalContext GoalContext) : RecipesContext(GoalContext, true)
{
    public override IEnumerable<RecipeDisplay> GetRecipeDisplay(Farmer who, ItemInfo itemInfo, bool showNeed)
    {
        if (who.recipesCooked.ContainsKey(itemInfo.Datum.ItemId) == showNeed)
            yield break;
        foreach (CraftingRecipe recipe in itemInfo.FromRecipe)
        {
            if (recipe.isCookingRecipe)
            {
                yield return new(
                    recipe,
                    itemInfo,
                    !who.cookingRecipes.ContainsKey(recipe.name),
                    GoalContext.PlayerOwned.OwnedRepr
                );
            }
        }
    }
}

public sealed record CraftingRecipesContext(GoalContext GoalContext) : RecipesContext(GoalContext, false)
{
    public override IEnumerable<RecipeDisplay> GetRecipeDisplay(Farmer who, ItemInfo itemInfo, bool showNeed)
    {
        foreach (CraftingRecipe recipe in itemInfo.FromRecipe)
        {
            if (recipe.isCookingRecipe)
            {
                continue;
            }
            if (who.craftingRecipes.TryGetValue(recipe.name, out int crafted))
            {
                if (crafted == 0 == showNeed)
                    yield return new(recipe, itemInfo, false, GoalContext.PlayerOwned.OwnedRepr);
            }
            else if (showNeed)
            {
                yield return new(recipe, itemInfo, true, GoalContext.PlayerOwned.OwnedRepr);
            }
        }
    }
}
