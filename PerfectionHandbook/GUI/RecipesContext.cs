using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(CraftingRecipe Recipe, ItemInfo Info, bool NotLearnt, PlayerOwned OwnedInfo)
{
    public readonly Color DisplayTint = GetDisplayTint(Recipe, NotLearnt, OwnedInfo);

    private static Color GetDisplayTint(CraftingRecipe recipe, bool notLearnt, PlayerOwned ownedInfo)
    {
        if (notLearnt)
            return Color.Black * 0.2f;
        if (!recipe.doesFarmerHaveIngredientsInInventory(ownedInfo.OwnedRepr))
            return Color.DimGray * 0.4f;
        return Color.White;
    }

    public SDUITooltipData Tooltip = new(
        " ",
        Recipe.DisplayName + ((Recipe.numberProducedPerCraft > 1) ? " x" + Recipe.numberProducedPerCraft : ""),
        CraftingRecipe: Recipe,
        AdditionalCraftingMaterials: OwnedInfo.OwnedRepr
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

    public void ClickMyFulfilment()
    {
        if (GoalContext.MyFulfillment.Who is not Farmer who)
            return;
        UpdateDisplayingFarmer(who);
    }

    public void ClickBestFulfilment()
    {
        if (GoalContext.BestFulfillment?.Who is not Farmer who)
            return;
        UpdateDisplayingFarmer(who);
    }

    private void UpdateDisplayingFarmer(Farmer who)
    {
        if (who != DisplayingFarmer)
        {
            DisplayingFarmer = who;
            ShowNeedToCraft = true;
        }
        else
        {
            ShowNeedToCraft = !ShowNeedToCraft;
        }
    }

    public IReadOnlyList<RecipeDisplay> FilteredRecipes
    {
        get
        {
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            List<RecipeDisplay> filteredItems = [];
            bool showNeedToCraft = ShowNeedToCraft;
            string txt = SearchText;
            foreach (ItemInfo itemInfo in ItemInfoCache.Cache.Values)
            {
                if (!string.IsNullOrEmpty(txt) && !itemInfo.Datum.DisplayName.ContainsIgnoreCase(txt))
                    continue;
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
                yield return new(recipe, itemInfo, !who.cookingRecipes.ContainsKey(recipe.name), GoalContext.OwnedInfo);
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
                    yield return new(recipe, itemInfo, false, GoalContext.OwnedInfo);
            }
            else if (showNeed)
            {
                yield return new(recipe, itemInfo, true, GoalContext.OwnedInfo);
            }
        }
    }
}
