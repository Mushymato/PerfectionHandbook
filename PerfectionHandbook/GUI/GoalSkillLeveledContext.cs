using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record ExpSquares(string Layout, bool Show, Color Tint);

public abstract partial record AbstractSkillDisplay(string SkillName, SDUISprite SkillIcon, int MaxLevel)
    : IPageDisplayEntry
{
    [Notify]
    protected int level = 0;

    [Notify]
    protected int expToNext = 0;
    protected int expToNextMax = 1;

    public bool Needed => Level < MaxLevel;

    public string DisplayCounts => I18n.Ui_Fulfillment_Dipslay(Level, MaxLevel);
    private static Color SkillColor1 = new(0xbd, 0x11, 0x4a);
    private static Color SkillColor2 = new(0x11, 0xbd, 0x84);
    public IReadOnlyList<IReadOnlyList<ExpSquares>> ExpToNextFillLayouts
    {
        get
        {
            List<ExpSquares> layouts = [];
            List<IReadOnlyList<ExpSquares>> layoutRows = [layouts];
            for (int i = 0; i < Level; i++)
            {
                layouts = AddSquare(layouts, layoutRows, i, 100);
            }
            if (Level < MaxLevel)
            {
                layouts = AddSquare(
                    layouts,
                    layoutRows,
                    Level,
                    100f * MathF.Max(expToNextMax - ExpToNext, 0) / expToNextMax
                );
                for (int i = Level + 1; i < MaxLevel; i++)
                {
                    layouts = AddSquare(layouts, layoutRows, i, 0);
                }
            }
            return layoutRows;
        }
    }

    private static List<ExpSquares> AddSquare(
        List<ExpSquares> layouts,
        List<IReadOnlyList<ExpSquares>> layoutRows,
        int i,
        float widthPercent
    )
    {
        if (i == 10)
        {
            layouts = [];
            layoutRows.Add(layouts);
        }
        layouts.Add(new ExpSquares($"{widthPercent}% stretch", widthPercent > 0, i < 10 ? SkillColor1 : SkillColor2));
        return layouts;
    }

    public bool SearchMatch(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return true;
        return SkillName.ContainsIgnoreCase(txt);
    }

    public void SetStatus(Farmer who)
    {
        Level = GetSkillLevel(who);
        if (Level < MaxLevel)
        {
            int exp = GetSkillExperience(who);
            int lvlExp = GetSkillExperienceForLevel(level);
            ExpToNext = exp - lvlExp;
            expToNextMax = GetSkillExperienceForLevel(level + 1) - lvlExp;
        }
        else
        {
            ExpToNext = 0;
            expToNextMax = 1;
        }
    }

    protected abstract int GetSkillExperienceForLevel(int level);

    protected abstract int GetSkillLevel(Farmer who);

    protected abstract int GetSkillExperience(Farmer who);
}

public sealed record VanillaSkillDisplay(int SkillIdx, int MaxLevel)
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
        MaxLevel
    )
{
    protected override int GetSkillExperienceForLevel(int level) => GoalSkillLeveledContext.GetExpForLevel(level);

    protected override int GetSkillLevel(Farmer who) => who.GetUnmodifiedSkillLevel(SkillIdx);

    protected override int GetSkillExperience(Farmer who) => who.experiencePoints[SkillIdx];
}

public sealed record SpacecoreSkillDisplay(string SkillId)
    : AbstractSkillDisplay(
        spaceCoreApi!.GetDisplayNameOfCustomSkill(SkillId),
        new(spaceCoreApi!.GetSkillIconForCustomSkill(SkillId)),
        10
    )
{
    internal static ISpaceCoreApi? spaceCoreApi = null;

    protected override int GetSkillExperienceForLevel(int level) => Farmer.getBaseExperienceForLevel(level);

    protected override int GetSkillLevel(Farmer who) => spaceCoreApi!.GetLevelForCustomSkill(who, SkillId);

    protected override int GetSkillExperience(Farmer who) => spaceCoreApi!.GetExperienceForCustomSkill(who, SkillId);
}

public sealed class GoalSkillLeveledContext(GoalContext goalCtx)
    : AbstractGoalPageListContext<AbstractSkillDisplay>(goalCtx)
{
    internal static int VanillaMaxLevel = 10;
    internal static Func<int, int> GetExpForLevel = Farmer.getBaseExperienceForLevel;

    internal static void Setup()
    {
        SpacecoreSkillDisplay.spaceCoreApi = ModEntry.help.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
        if (
            ModEntry.help.ModRegistry.GetApi<IVanillaPlusProfessions>("KediDili.VanillaPlusProfessions")
            is IVanillaPlusProfessions vppApi
        )
        {
            VanillaMaxLevel = 20;
            GetExpForLevel = (level) =>
            {
                if (level > 10)
                    return vppApi.LevelExperiences[level - 10];
                return Farmer.getBaseExperienceForLevel(level);
            };
        }
    }

    protected override IReadOnlyList<AbstractSkillDisplay> MakeAllDisplay()
    {
        List<AbstractSkillDisplay> skillDisplays = [];

        for (int i = 0; i < 5; i++)
        {
            skillDisplays.Add(new VanillaSkillDisplay(i, VanillaMaxLevel));
        }

        if (SpacecoreSkillDisplay.spaceCoreApi != null)
        {
            foreach (string scSkill in SpacecoreSkillDisplay.spaceCoreApi.GetCustomSkills())
            {
                skillDisplays.Add(new SpacecoreSkillDisplay(scSkill));
            }
        }

        return skillDisplays;
    }
}
