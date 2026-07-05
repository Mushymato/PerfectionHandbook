using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public partial record IngredientDisplay(NeededForInfoGroup NeededFor, int OwnedCount)
    : AbstractItemCountDisplay(NeededFor.ReprInfo, OwnedCount)
{
    [Notify]
    public int neededCount = 0;
    public override bool Needed => NeededCount > 0;
    public Color DigitTint => Count >= NeededCount ? Color.LimeGreen : Color.White;
    private List<NeededForInfo> notYetCrafted = [];

    public override Color DisplayTint =>
        OwnedCount >= NeededCount ? HandbookContext.ActiveColor : HandbookContext.InactiveColor;

    public override void SetStatus(Farmer who)
    {
        notYetCrafted = NeededFor.GetNotYetCrafted(who);
        NeededCount = notYetCrafted.Sum(notYet => notYet.Count);
        UpdateCount();
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public override SDUITooltipData Tooltip =>
        new(
            GetTooltipDesc(),
            Title: NeededFor.CraftingDesc,
            Item: NeededFor.ReprInfo.ReprItem,
            RequiredItemAmount: NeededCount
        );

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
}

public sealed class GoalRecipesIngredientContext(IGoalContext goalCtx)
    : AbstractItemCountContext<IngredientDisplay>(goalCtx, canToggleNeeded: false, canToggleCountMode: false)
{
    protected override IReadOnlyList<IngredientDisplay> MakeAllDisplay()
    {
        List<IngredientDisplay> displayList = [];
        foreach (NeededForInfoGroup neededForInfoGroup in ItemInfoCache.NeededForRecipe.Values)
        {
            int ownedCount = neededForInfoGroup.GetOwned(GoalCtx.OwnedInfo);
            displayList.Add(new(neededForInfoGroup, ownedCount));
        }
        return displayList
            .OrderBy(display => TokenParser.ParseText(display.NeededFor.ReprInfo.Datum.DisplayName))
            .ToList();
    }
}
