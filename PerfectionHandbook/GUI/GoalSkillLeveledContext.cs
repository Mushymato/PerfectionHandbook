using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record ExpSquares(string Layout, bool Show);

public abstract partial record AbstractSkillDisplay(string SkillName, SDUISprite SkillIcon, int MaxLevel, int MaxExp)
    : IPageDisplayEntry
{
    [Notify]
    protected int level = 0;

    [Notify]
    protected int expToNext = 0;
    protected int expToNextMax = 1;

    public bool Needed => Level < MaxLevel;

    public string DisplayCounts => I18n.Ui_Fulfillment_Dipslay(Level, MaxLevel);
    public IReadOnlyList<ExpSquares> ExpToNextFillLayouts
    {
        get
        {
            float widthMult = MaxLevel > 10 ? 50f : 100f;
            List<ExpSquares> layouts = [];
            for (int i = 0; i < Level; i++)
                layouts.Add(new ExpSquares($"{widthMult}% stretch", true));
            layouts.Add(
                new ExpSquares($"{widthMult * MathF.Min(ExpToNext, expToNextMax) / expToNextMax}% stretch", true)
            );
            for (int i = Level + 1; i < MaxLevel; i++)
                layouts.Add(new ExpSquares(string.Empty, false));
            return layouts;
        }
    }

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return SkillName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        int[] expLvls = GetExpLevels(MaxLevel);
        Level = GetSkillLevel(who);
        int exp = GetSkillExperience(who);
        for (int i = 0; i < Math.Min(Level, expLvls.Length); i++)
            exp -= expLvls[i];
        if (Level < expLvls.Length - 1)
            expToNextMax = expLvls[Level];
        ExpToNext = exp;
    }

    protected abstract int[] GetExpLevels(int maxLevel);

    protected abstract int GetSkillLevel(Farmer who);

    protected abstract int GetSkillExperience(Farmer who);
}

public sealed record VanillaSkillDisplay(int SkillIdx, int MaxLevel, int MaxExp)
    : AbstractSkillDisplay(
        Farmer.getSkillDisplayNameFromIndex(SkillIdx),
        new(
            Game1.buffsIcons,
            SkillIdx switch
            {
                0 => new Rectangle(0, 0, 16, 16),
                1 => new Rectangle(16, 0, 16, 16),
                2 => new Rectangle(80, 0, 16, 16),
                3 => new Rectangle(32, 0, 16, 16),
                4 => new Rectangle(128, 16, 16, 16),
                _ => new Rectangle(64, 0, 16, 16),
            }
        ),
        MaxLevel,
        MaxExp
    )
{
    protected override int[] GetExpLevels(int maxLevel) => GoalSkillLeveledContext.GetExpLevels(MaxLevel);

    protected override int GetSkillLevel(Farmer who) => who.GetUnmodifiedSkillLevel(SkillIdx);

    protected override int GetSkillExperience(Farmer who) => who.experiencePoints[SkillIdx];
}

public sealed record SpacecoreSkillDisplay(string SkillId, int MaxLevel, int MaxExp)
    : AbstractSkillDisplay(
        spaceCoreApi!.GetDisplayNameOfCustomSkill(SkillId),
        new(spaceCoreApi!.GetSkillIconForCustomSkill(SkillId)),
        MaxLevel,
        MaxExp
    )
{
    internal static ISpaceCoreApi? spaceCoreApi = null;

    protected override int[] GetExpLevels(int maxLevel) => GoalSkillLeveledContext.ExpPerLevel;

    protected override int GetSkillLevel(Farmer who) => spaceCoreApi!.GetLevelForCustomSkill(who, SkillId);

    protected override int GetSkillExperience(Farmer who) => spaceCoreApi!.GetExperienceForCustomSkill(who, SkillId);
}

public sealed class GoalSkillLeveledContext(GoalContext goalCtx)
    : AbstractGoalPageListContext<AbstractSkillDisplay>(goalCtx)
{
    internal const int MaxExp = 15000;
    internal static int[] ExpPerLevel = [100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000];
    internal static int[]? ExpPerLevelVpp;

    internal static int[] GetExpLevels(int maxLvl) =>
        maxLvl > 10 && ExpPerLevelVpp != null ? ExpPerLevelVpp : ExpPerLevel;

    internal static void Setup()
    {
        SpacecoreSkillDisplay.spaceCoreApi = ModEntry.help.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
        if (
            ModEntry.help.ModRegistry.GetApi<IVanillaPlusProfessions>("KediDili.VanillaPlusProfessions")
            is IVanillaPlusProfessions vppApi
        )
            ExpPerLevelVpp = [.. ExpPerLevel, .. vppApi.LevelExperiences];
    }

    protected override IReadOnlyList<AbstractSkillDisplay> MakeAllDisplay()
    {
        List<AbstractSkillDisplay> skillDisplays = [];

        for (int i = 0; i < 5; i++)
        {
            skillDisplays.Add(
                new VanillaSkillDisplay(
                    i,
                    (ExpPerLevelVpp ?? ExpPerLevel).Length,
                    (ExpPerLevelVpp ?? ExpPerLevel).Max()
                )
            );
        }

        if (SpacecoreSkillDisplay.spaceCoreApi != null)
        {
            foreach (string scSkill in SpacecoreSkillDisplay.spaceCoreApi.GetCustomSkills())
            {
                skillDisplays.Add(new SpacecoreSkillDisplay(scSkill, ExpPerLevel.Length, ExpPerLevel.Max()));
            }
        }

        return skillDisplays;
    }
}
