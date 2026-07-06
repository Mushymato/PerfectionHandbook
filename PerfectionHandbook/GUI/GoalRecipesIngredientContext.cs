using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public partial record IngredientDisplay(string Key, NeededForInfoGroup NeededFor, int OwnedCount)
    : AbstractItemCountDisplay(NeededFor.ReprInfo, OwnedCount)
{
    [Notify]
    public int neededCount = 0;
    public override bool Needed => NeededCount > 0;
    public Color DigitTint => Count >= NeededCount ? Color.LimeGreen : Color.White;
    private List<NeededForInfo> notYetCrafted = [];
    public SDUISprite Repr =
        NeededFor.ReprIcon ?? new(NeededFor.ReprInfo.Datum.GetTexture(), NeededFor.ReprInfo.Datum.GetSourceRect());

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

    public override bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        if (NeededFor.CraftingDesc.ContainsIgnoreCase(txt))
            return true;
        return base.SearchMatch(txt);
    }
}

public sealed class GoalRecipesIngredientContext(IGoalContext goalCtx)
    : AbstractItemCountContext<IngredientDisplay>(goalCtx, canToggleNeeded: false, canToggleCountMode: false)
{
    protected override IReadOnlyList<IngredientDisplay> MakeAllDisplay()
    {
        List<IngredientDisplay> displayList = [];
        foreach ((string key, NeededForInfoGroup neededForInfoGroup) in ItemInfoCache.NeededForRecipe)
        {
            int ownedCount = neededForInfoGroup.GetOwned(GoalCtx.OwnedInfo);
            displayList.Add(new(key, neededForInfoGroup, ownedCount));
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
            SORTMODE_NAME => displayList.OrderBy(static disp => disp.NeededFor.CraftingDesc).ToList(),
            SORTMODE_COUNT => displayList
                .OrderByDescending(static disp => (disp.NeededCount <= disp.OwnedCount ? 1 : 0, disp.OwnedCount))
                .ToList(),
            _ => base.SortAllDisplay(displayList),
        };
    }
}
