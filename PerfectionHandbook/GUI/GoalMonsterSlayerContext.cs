using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.TokenizableStrings;

namespace PerfectionHandbook.GUI;

public sealed partial record MonsterSlayerDisplay(
    string Id,
    string DisplayName,
    SDUISprite DisplaySprite,
    IReadOnlyList<string> QuestTargets,
    int RequiredCount
) : IPageDisplayEntry
{
    public override int GetHashCode() => Id.GetHashCode();

    [Notify]
    public int killedCount = 0;

    [Notify]
    public bool isExpanded = false;

    public bool Needed => KilledCount < RequiredCount;
    public ReminderEntry? Reminder =>
        MenuHandler.Reminders.GetOrCreateEntry(ReminderEntryFactory.Kind_MonsterSlayer, Id);

    public string DisplayCounts => I18n.Ui_Fulfillment_Dipslay(KilledCount, RequiredCount);

    public string QuestFillLayout =>
        RequiredCount <= 0
            ? "100% stretch"
            : $"{100f * MathF.Min(KilledCount, RequiredCount) / RequiredCount}% stretch";

    public string TooltipText =>
        string.Concat(
            DisplayName,
            ": ",
            DisplayCounts,
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
        if (DisplayName.ContainsIgnoreCase(txt))
            return true;
        if (QuestTargets.Any(target => target.ContainsIgnoreCase(txt)))
            return true;
        return false;
    }

    public void ToggleReminder() => MenuHandler.Reminders.ToggleEntryKeyChecked(Reminder);
}

public sealed class GoalMonsterSlayerContext(IGoalContext goalCtx)
    : AbstractPageListContext<MonsterSlayerDisplay>(goalCtx)
{
    protected override IReadOnlyList<MonsterSlayerDisplay> MakeAllDisplay()
    {
        Dictionary<string, string> monsterData = DataLoader.Monsters(Game1.content);
        List<MonsterSlayerDisplay> slayerDisplay = [];
        foreach (
            (string slayerId, MonsterSlayerQuestData slayerQuestData) in DataLoader.MonsterSlayerQuests(Game1.content)
        )
        {
            List<string> targets = [];
            foreach (string targetMonster in slayerQuestData.Targets)
            {
                if (monsterData.TryGetValue(targetMonster, out string? monsterDataStr))
                {
                    string[] parts = monsterDataStr.Split('/');
                    if (parts.Length > 14)
                    {
                        targets.Add(parts[14]);
                        continue;
                    }
                }
                targets.Add(targetMonster);
            }
            slayerDisplay.Add(
                new(
                    slayerId,
                    TokenParser.ParseText(slayerQuestData.DisplayName) ?? slayerId,
                    GetMonsterDisplaySprite(slayerQuestData),
                    targets,
                    slayerQuestData.Count
                )
            );
        }
        return slayerDisplay;
    }

    internal static SDUISprite GetMonsterDisplaySprite(MonsterSlayerQuestData slayerQuestData)
    {
        if (
            slayerQuestData.Targets?.FirstOrDefault(name =>
                Game1.content.DoesAssetExist<Texture2D>($"Characters\\Monsters\\{name}")
            )
            is string firstTarget
        )
        {
            return new(DrawHelper.SafeLoad($"Characters\\Monsters\\{firstTarget}"), GetMonsterSourceRect(firstTarget));
        }
        return new(Game1.mouseCursors, new(0, 0, 16, 16));
    }

    /*
    Based on:
    https://github.com/focustense/StardewUI/blob/7357664646a4d93bad533d92f66c859d2b94c2ac/TestMod/Examples/MonsterViewModel.cs#L102

    MIT License

    Copyright (c) 2024 focustense

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
    private static Rectangle GetMonsterSourceRect(string name)
    {
        return name switch
        {
            "Bat"
            or "Frost Bat"
            or "Lava Bat"
            or "Iridium Bat"
            or "Ghost"
            or "Carbon Ghost"
            or "Putrid Ghost"
            or "Fly"
            or "Grub"
            or "Stone Golem"
            or "Wilderness Golem"
            or "Iridium Golem"
            or "False Magma Cap"
            or "Dust Spirit"
            or "Shadow Shaman" => new(0, 0, 16, 24),

            "Duggy" or "Magma Duggy" => new(0, 24, 16, 24),

            "Rock Crab" or "Lava Crab" or "Iridium Crab" or "Truffle Crab" => new(16, 0, 16, 24),

            "Blue Squid" => new(0, 0, 24, 24),

            "Green Slime"
            or "Tiger Slime"
            or "Skeleton"
            or "Skeleton Mage"
            or "Mummy"
            or "Shadow Brute"
            or "Shadow Guy" => new(0, 0, 16, 32),

            "Shadow Sniper" or "Big Slime" or "Pepper Rex" or "Spider" or "Serpent" or "Royal Serpent" => new(
                0,
                0,
                32,
                32
            ),

            "Crow" => new(0, 0, 64, 64),

            _ => new(0, 0, 16, 16),
        };
    }
}
