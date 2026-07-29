using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed record RecipeDisplay(
    ItemInfo Info,
    int OwnedCount,
    CraftingRecipe Recipe,
    IReadOnlyList<(NeededForInfoGroup, NeededForInfo)> Needs,
    PlayerOwned OwnedInfo
) : AbstractItemCountDisplay(Info, OwnedCount)
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

    public override ReminderEntry? Reminder
    {
        get
        {
            if (base.Reminder is not ReminderEntry entry)
                return null;
            List<ReminderEntry> subReminders = [];
            foreach ((NeededForInfoGroup group, NeededForInfo need) in Needs)
            {
                subReminders.Add(
                    new(string.Concat(entry, '.', group.RawId), group.Repr, group.CraftingDesc, need.Count)
                    {
                        IsSub = true,
                    }
                );
            }
            entry.SubReminders = subReminders;
            return entry;
        }
    }

    public override string ReminderKey => Recipe.isCookingRecipe ? "CookingRecipe" : "CraftingRecipe";
    public override string ReminderText =>
        Recipe.isCookingRecipe
            ? I18n.Reminder_Verb_Cook(Info.Datum.DisplayName)
            : I18n.Reminder_Verb_Craft(Info.Datum.DisplayName);
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
                    recipeDisplayList.Add(new(itemInfo, ownedCount, recipe.Recipe, recipe.Needs, GoalCtx.OwnedInfo));
                }
            }
        }
        return recipeDisplayList;
    }
}
