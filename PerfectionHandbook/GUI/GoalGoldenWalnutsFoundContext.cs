using PerfectionHandbook.GUI.Shared;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record GoldenWalnutsFoundDisplay(string Key, int Count = 1, int MaxCount = 1) : IPageDisplayEntry
{
    public string Name = I18n.GetByKey(string.Concat("GoldenWalnut.Name.", Key));
    public string Hint = I18n.GetByKey(string.Concat("GoldenWalnut.Hint.", Key));
    public string CountText = I18n.Ui_Fulfillment_Dipslay(Count, MaxCount);
    public bool Needed { get; } = Count < MaxCount;

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return Name.ContainsIgnoreCase(txt) || Hint.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who) { }
}

public sealed class GoalGoldenWalnutsFoundContext(IGoalContext goalCtx)
    : AbstractPageListContext<GoldenWalnutsFoundDisplay>(goalCtx)
{
    /*
      Based on:
      https: //github.com/MouseyPounds/stardew-checkup/blob/8e48aa1806ad2c856d35e1a68f08128b4673f2c5/stardew-checkup.js#L4458

      MIT License

      Copyright (c) 2017 MouseyPounds

      Permission is hereby granted, free of charge, to any person obtaining a copy
      of this software and associated documentation files (the "Software"), to deal
      in the Software without restriction, including without limitation the rights
      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
      copies of the Software, and to permit persons to whom the Software is
      furnished to do so, subject to the following conditions:

      The above copyright notice and this permission notice shall be included in all
      copies or substantial portions of the Software.

      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
      SOFTWARE.
      */
    protected override IReadOnlyList<GoldenWalnutsFoundDisplay> MakeAllDisplay()
    {
        // hardcoding hell
        List<GoldenWalnutsFoundDisplay> displays = [];

        // special: golden coconut
        displays.Add(new("GoldenCoconut", Game1.netWorldState.Value.GoldenCoconutCracked ? 1 : 0, 1));

        // limited: may drop multiple times up to a limit
        MakeLimitedWalnut(displays, "Birdie", 5);
        MakeLimitedWalnut(displays, "Darts", 3);
        MakeLimitedWalnut(displays, "TigerSlimeNut");
        MakeLimitedWalnut(displays, "VolcanoNormalChest");
        MakeLimitedWalnut(displays, "VolcanoRareChest");
        MakeLimitedWalnut(displays, "VolcanoBarrel", 5);
        MakeLimitedWalnut(displays, "VolcanoMining", 5);
        MakeLimitedWalnut(displays, "VolcanoMonsterDrop", 5);
        MakeLimitedWalnut(displays, "IslandFarming", 5);
        MakeLimitedWalnut(displays, "MusselStone", 5);
        MakeLimitedWalnut(displays, "IslandFishing", 5);
        MakeLimitedWalnut(displays, "Island_N_BuriedTreasureNut");
        MakeLimitedWalnut(displays, "Island_W_BuriedTreasureNut");
        MakeLimitedWalnut(displays, "Island_W_BuriedTreasureNut2");

        // one time: get these once
        MakeOneTimeWalnut(displays, "Bush_IslandEast_17_37");
        MakeOneTimeWalnut(displays, "Bush_IslandShrine_23_34");
        MakeOneTimeWalnut(displays, "Bush_IslandSouth_31_5");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_9_84");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_20_26");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_56_27");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_4_42");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_45_38");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_47_40");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_13_33");
        MakeOneTimeWalnut(displays, "Bush_IslandNorth_5_30");
        MakeOneTimeWalnut(displays, "Bush_Caldera_28_36");
        MakeOneTimeWalnut(displays, "Bush_Caldera_9_34");
        MakeOneTimeWalnut(displays, "Bush_CaptainRoom_2_4");
        MakeOneTimeWalnut(displays, "TreeNut");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_19_39");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_19_13");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_57_79");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_54_21");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_42_77");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_62_54");
        MakeOneTimeWalnut(displays, "Buried_IslandNorth_26_81");
        MakeOneTimeWalnut(displays, "IslandLeftPlantRestored");
        MakeOneTimeWalnut(displays, "IslandRightPlantRestored");
        MakeOneTimeWalnut(displays, "IslandBatRestored");
        MakeOneTimeWalnut(displays, "IslandFrogRestored");
        MakeOneTimeWalnut(displays, "IslandCenterSkeletonRestored", 6);
        MakeOneTimeWalnut(displays, "IslandSnakeRestored", 3);
        MakeOneTimeWalnut(displays, "Bush_IslandWest_104_3");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_31_24");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_38_56");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_75_29");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_64_30");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_54_18");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_25_30");
        MakeOneTimeWalnut(displays, "Bush_IslandWest_15_3");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_21_81");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_62_76");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_39_24");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_88_14");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_43_74");
        MakeOneTimeWalnut(displays, "Buried_IslandWest_30_75");
        MakeOneTimeWalnut(displays, "IslandWestCavePuzzle", 3);
        MakeOneTimeWalnut(displays, "SandDuggy");
        MakeOneTimeWalnut(displays, "TreeNutShot");
        MakeOneTimeWalnut(displays, "Mermaid");
        MakeOneTimeWalnut(displays, "Buried_IslandSouthEastCave_36_26");
        MakeOneTimeWalnut(displays, "Buried_IslandSouthEast_25_17");
        MakeOneTimeWalnut(displays, "StardropPool");
        MakeOneTimeWalnut(displays, "BananaShrine", 3);
        MakeOneTimeWalnut(displays, "IslandGourmand1", 5);
        MakeOneTimeWalnut(displays, "IslandGourmand2", 5);
        MakeOneTimeWalnut(displays, "IslandGourmand3", 5);
        MakeOneTimeWalnut(displays, "IslandShrinePuzzle", 5);

        return displays;
    }

    private static void MakeOneTimeWalnut(List<GoldenWalnutsFoundDisplay> displays, string key, int maxCount = 1)
    {
        displays.Add(
            new(key, Count: Game1.player.team.collectedNutTracker.Contains(key) ? maxCount : 0, MaxCount: maxCount)
        );
    }

    private static void MakeLimitedWalnut(List<GoldenWalnutsFoundDisplay> displays, string key, int maxCount = 1)
    {
        displays.Add(new(key, Count: Game1.player.team.GetDroppedLimitedNutCount(key), maxCount));
    }
}
