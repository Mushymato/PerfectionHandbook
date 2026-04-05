using StardewValley;

namespace PerfectionHandbook.Integration;

public record SDUITooltipData(
    string Text,
    string? Title = null,
    Item? Item = null,
    int? CurrencyAmount = null,
    int CurrencySymbol = 0,
    string? RequiredItemId = null,
    int RequiredItemAmount = 0,
    CraftingRecipe? CraftingRecipe = null,
    IList<Item>? AdditionalCraftingMaterials = null
);
