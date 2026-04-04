using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewValley;

namespace PerfectionHandbook.GUI;

public sealed partial record ItemShippedContext(GoalContext GoalContext) : IGoalPageContext
{
    public const int MAX_SHOWN = 1000;

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

    public IReadOnlyList<ItemInfo> FilteredNeedToShip
    {
        get
        {
            Farmer? who = DisplayingFarmer;
            if (who == null)
                return [];
            bool showNeedToShip = ShowNeedToShip;
            int shownCnt = MAX_SHOWN;
            List<ItemInfo> filteredNeedToShip = [];
            string txt = SearchText;
            foreach (
                ItemInfo itemInfo in ItemCache.Cache.Where(
                    (itemInfo) =>
                    {
                        if (who.basicShipped.TryGetValue(itemInfo.Datum.ItemId, out int shippedCount))
                            return shippedCount <= 0 == showNeedToShip;
                        return showNeedToShip;
                    }
                )
            )
            {
                if (!itemInfo.IsPotentialShipped)
                    continue;
                if (!string.IsNullOrEmpty(txt) && !itemInfo.Datum.DisplayName.Contains(txt))
                    continue;
                filteredNeedToShip.Add(itemInfo);
                shownCnt--;
                if (shownCnt == 0)
                    break;
            }
            return filteredNeedToShip;
        }
    }
}
