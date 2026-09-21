using System.ComponentModel;
using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public partial record IngredientDisplay(string Key, NeededForInfoGroup NeededFor, int OwnedCount, int OwnedCountFridge)
    : AbstractItemCountDisplay(NeededFor.ReprInfo, OwnedCount)
{
    [Notify]
    public int neededCount = 0;
    public override bool Needed => NeededCount > 0;
    public Color DigitTint => Count >= NeededCount ? Color.LimeGreen : Color.White;
    private List<NeededForInfo> notYetCrafted = [];
    private RecipeMode lastRecipeMode = RecipeMode.Both;

    public override Color DisplayTint =>
        OwnedCount >= NeededCount ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public override void SetStatus(Farmer who)
    {
        RefreshNotYetCrafted(who, lastRecipeMode);
        UpdateCount();
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public void RefreshNotYetCrafted(Farmer who, RecipeMode recipeMode)
    {
        if (recipeMode == RecipeMode.Cooking)
            Count = OwnedCountFridge;
        else
            Count = OwnedCount;
        lastRecipeMode = recipeMode;
        notYetCrafted = NeededFor.GetNotYetCrafted(who, lastRecipeMode);
        NeededCount = notYetCrafted.Sum(notYet => notYet.Count);
    }

    public override SDUITooltipData Tooltip =>
        new(
            GetTooltipDesc(),
            Title: NeededFor.CraftingDesc,
            Item: NeededFor.ReprInfo.ReprItem,
            RequiredItemAmount: NeededCount
        );

    public string ScreenRead => I18n.Screenread_Ingredient(NeededCount, NeededFor.ReprInfo.ReprItem, OwnedCount);

    private const string SPACER = "  ";

    public override string GetTooltipDesc()
    {
        List<string> recipeNames = [];
        foreach (NeededForInfo notYet in notYetCrafted)
        {
            recipeNames.Add(notYet.Recipe.DisplayName);
            if (recipeNames.Count >= 9)
            {
                recipeNames.Add(I18n.Ui_Ingredients_AndMore());
                break;
            }
        }
        return string.Concat(
            I18n.Ui_Misc_NeededFor(Count, NeededCount),
            Environment.NewLine,
            SPACER,
            string.Join(string.Concat(Environment.NewLine, SPACER), recipeNames),
            Environment.NewLine,
            I18n.Ui_Ingredients_Total(notYetCrafted.Count)
        );
    }

    public override bool SearchMatch(string txt)
    {
        if (NeededFor.CraftingDesc.ContainsIgnoreCase(txt))
            return true;
        return base.SearchMatch(txt);
    }

    public override ReminderEntry? Reminder { get; } =
        MenuHandler.Reminders.GetOrCreateEntry(ReminderEntryFactory.Kind_RecipesIngredient, Key);
}

public sealed partial class GoalRecipesIngredientContext : AbstractItemCountContext<IngredientDisplay>
{
    public GoalRecipesIngredientContext(IGoalContext goalCtx)
        : base(goalCtx, canToggleNeeded: false, canToggleCountMode: false, itemPerPageModifier: 6.0 / 13.0)
    {
        PropertyChanged += OnPropertyChanged_RecipeMode;
    }

    private void OnPropertyChanged_RecipeMode(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RecipeMode))
            SetAllRecipeMode();
    }

    private void SetAllRecipeMode()
    {
        foreach (IngredientDisplay display in AllDisplay)
        {
            display.RefreshNotYetCrafted(GoalCtx.Who, recipeMode);
        }
        ReSortFilteredDisplay();
    }

    [Notify]
    private RecipeMode recipeMode = RecipeMode.Both;
    public int RecipeModeIndex
    {
        get => (int)RecipeMode;
        set => RecipeMode = (RecipeMode)value;
    }

    protected override IReadOnlyList<IngredientDisplay> MakeAllDisplay()
    {
        List<IngredientDisplay> displayList = [];
        foreach ((string key, NeededForInfoGroup neededForInfoGroup) in ItemInfoCache.NeededForRecipe)
        {
            int inventoryCount = GoalCtx.Who.Items.CountId(key);
            displayList.Add(
                new(
                    key,
                    neededForInfoGroup,
                    neededForInfoGroup.GetOwned(GoalCtx.OwnedInfo, false) + inventoryCount,
                    neededForInfoGroup.GetOwned(GoalCtx.OwnedInfo, true) + inventoryCount
                )
            );
        }
        return displayList;
    }

    protected override List<IngredientDisplay> SortAllDisplay(List<IngredientDisplay> displayList)
    {
        return SortMode switch
        {
            SORTMODE_DEFAULT => displayList
                .OrderBy(static disp =>
                    (disp.Key.StartsWith($"{ModEntry.ModId}/") ? -1024 : disp.Info.Datum.Category, disp.Key)
                )
                .ToList(),
            SORTMODE_NAME => displayList
                .OrderBy(static disp => disp.NeededFor.CraftingDesc, ModEntry.displayStringComparer)
                .ToList(),
            SORTMODE_COUNT => displayList
                .OrderByDescending(static disp => (disp.NeededCount <= disp.OwnedCount ? 1 : 0, disp.OwnedCount))
                .ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }
}
