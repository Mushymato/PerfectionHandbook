using PerfectionHandbook.GUI.Shared;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed partial record MonsterSlayerDisplay(string QuestName, List<string> QuestTargets, int RequiredCount)
    : IPageDisplayEntry
{
    [Notify]
    public int killedCount = 0;

    [Notify]
    public bool isExpanded = false;

    public bool Needed => KilledCount < RequiredCount;

    public string DisplayCounts => I18n.Ui_Fulfillment_Dipslay(KilledCount, RequiredCount);

    public string QuestFillLayout => $"{(float)KilledCount / RequiredCount * 100}% stretch";

    public string TooltipText =>
        string.Concat(
            DisplayCounts,
            ":",
            Environment.NewLine,
            string.Join(Environment.NewLine, QuestTargets.Select(GetMonsterName))
        );

    private string GetMonsterName(string name)
    {
        if (!DataLoader.Monsters(Game1.content).TryGetValue(name, out string? monsterStr))
        {
            return name;
        }
        string[] array = monsterStr.Split('/');
        if (!ArgUtility.TryGet(array, 14, out string displayName, out _))
        {
            return name;
        }
        return displayName;
    }

    public void SetStatus(Farmer who)
    {
        KilledCount = QuestTargets.Sum(target => who.stats.getMonstersKilled(target));
    }

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        if (QuestName.ContainsIgnoreCase(txt))
            return true;
        if (QuestTargets.Any(target => target.ContainsIgnoreCase(txt)))
            return true;
        return false;
    }
}

public sealed class GoalMonsterSlayerContext(GoalContext goalCtx)
    : AbstractGoalPageListContext<MonsterSlayerDisplay>(goalCtx)
{
    protected override IReadOnlyList<MonsterSlayerDisplay> MakeAllDisplay()
    {
        List<MonsterSlayerDisplay> slayerDisplay = [];
        foreach (MonsterSlayerQuestData slayerQuestData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
        {
            slayerDisplay.Add(
                new(TokenParser.ParseText(slayerQuestData.DisplayName), slayerQuestData.Targets, slayerQuestData.Count)
            );
        }
        return slayerDisplay;
    }
}
