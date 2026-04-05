using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record ItemShippedDisplay(ItemInfo Info, bool OwnedAny)
{
    public readonly Color DisplayTint = (OwnedAny ? 1f : 0.5f) * Color.White;
}

public sealed partial record ItemShippedContext(GoalContext GoalContext) : IGoalPageContext
{
    [Notify]
    public string searchText = string.Empty;

    [Notify]
    private Farmer? displayingFarmer = GoalContext.MyFulfillment.Who;

    [Notify]
    private bool showNeedToShip = true;

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
            ShowNeedToShip = true;
        }
        else
        {
            ShowNeedToShip = !ShowNeedToShip;
        }
    }

    public IReadOnlyList<ItemShippedDisplay> FilteredItems
    {
        get
        {
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            bool showNeedToShip = ShowNeedToShip;
            int shownCnt = HandbookContext.MAX_SHOWN;
            List<ItemShippedDisplay> filteredNeedToShip = [];
            string txt = SearchText;
            foreach (
                ItemInfo itemInfo in ItemInfoCache.Cache.Values.Where(
                    (itemInfo) => !who.basicShipped.ContainsKey(itemInfo.Datum.ItemId) == showNeedToShip
                )
            )
            {
                if (!itemInfo.IsPotentialShipped)
                    continue;
                if (!string.IsNullOrEmpty(txt) && !itemInfo.Datum.DisplayName.ContainsIgnoreCase(txt))
                    continue;
                filteredNeedToShip.Add(
                    new(itemInfo, GoalContext.OwnedInfo.OwnedGroups.ContainsKey(itemInfo.Datum.QualifiedItemId))
                );
                shownCnt--;
                if (shownCnt == 0)
                    break;
            }
            return filteredNeedToShip;
        }
    }
}
