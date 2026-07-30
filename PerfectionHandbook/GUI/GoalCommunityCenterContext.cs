using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace PerfectionHandbook.GUI;

public sealed partial record CommunityCenterBundleIngredient(ItemInfo Info, int Count, int Quality, bool Complete)
{
    public bool HasCount => Count > 1;
    public SDUISprite IngredientBorder =>
        new(Game1.content.Load<Texture2D>("LooseSprites\\JunimoNote"), new(Complete ? 620 : 512, 244, 18, 18));
    public SDUISprite? QualityStar = DrawHelper.GetQualityStar(Quality);
    public bool HasQualityStar => QualityStar != null;
}

public sealed record CommunityCenterBundleDisplay(
    bool Needed,
    string BundleName,
    string BundleCompletionText,
    SDUISprite BundleIcon,
    IReadOnlyList<CommunityCenterBundleIngredient> BundleIngredients
) : IPageDisplayEntry
{
    public ReminderEntry? Reminder => throw new NotImplementedException();

    public bool SearchMatch(string txt)
    {
        return BundleName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who) { }
}

public sealed class GoalCommunityCenterContext(IGoalContext goalCtx)
    : AbstractPageListContext<CommunityCenterBundleDisplay>(goalCtx)
{
    protected override IReadOnlyList<CommunityCenterBundleDisplay> MakeAllDisplay()
    {
        List<CommunityCenterBundleDisplay> bundleDisplay = [];
        foreach ((string bundleKey, string bundleData) in Game1.netWorldState.Value.BundleData)
        {
            int bundleId = Convert.ToInt32(bundleKey.Split('/')[1]);
            if (!Game1.netWorldState.Value.Bundles.TryGetValue(bundleId, out bool[] completion))
            {
                continue;
            }
            // creating this to steal some parsing code
            Bundle bundle = new(bundleId, bundleData, completion, Point.Zero, "LooseSprites\\JunimoNote", null);
            // bundle.ingredients
            List<CommunityCenterBundleIngredient> bundleIngredients = [];
            foreach (BundleIngredientDescription ingredient in bundle.ingredients)
            {
                if (string.IsNullOrEmpty(ingredient.id))
                    continue;
                // no support for category for now
                string qId = ItemRegistry.ManuallyQualifyItemId(ingredient.id, "(O)");
                if (!ItemInfoCache.Cache.TryGetValue(qId, out ItemInfo? info))
                {
                    info = new ItemInfo(ItemRegistry.GetDataOrErrorItem(qId));
                }
                bundleIngredients.Add(new(info, ingredient.stack, ingredient.quality, ingredient.completed));
            }
            if (bundleIngredients.Count == 0)
                continue;
            int idx = bundle.bundleTextureIndexOverride >= 0 ? bundle.bundleTextureIndexOverride : bundle.bundleIndex;
            Texture2D tx = bundle.bundleTextureOverride ?? Game1.content.Load<Texture2D>("LooseSprites\\JunimoNote");
            int yOffset = bundle.bundleTextureOverride == null ? 180 : 0;
            CommunityCenterBundleDisplay display = new(
                !bundle.complete,
                bundle.label,
                I18n.Ui_Fulfillment_Dipslay(
                    bundle.ingredients.Count(ing => ing.completed),
                    bundle.numberOfIngredientSlots
                ),
                new(tx, new Rectangle(idx * 16 * 2 % tx.Width, yOffset + 32 * (idx * 16 * 2 / tx.Width), 32, 32)),
                bundleIngredients
            );
            bundleDisplay.Add(display);
        }
        return bundleDisplay;
    }
}
