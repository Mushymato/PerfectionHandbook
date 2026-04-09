using Microsoft.Xna.Framework;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;

namespace PerfectionHandbook.GUI;

public sealed record LevelSquare(bool Show, Color Tint);

public abstract partial record AbstractSkillDisplay(string SkillName, SDUISprite SkillIcon, int MaxLevel)
    : IPageDisplayEntry
{
    [Notify]
    protected int level = 0;

    [Notify]
    protected int expToNext = 0;
    protected int expToNextMax = 1;

    public bool Needed => Level < MaxLevel;

    // public string DisplayCounts => I18n.Ui_Fulfillment_Dipslay(Level, MaxLevel);
    public string SkillCountDisplay =>
        string.Concat(SkillName, Environment.NewLine, I18n.Ui_Skill_Dipslay(Level, MaxLevel));
    private static Color SkillColor1 = new(0xbd, 0x11, 0x4a);
    private static Color SkillColor2 = new(0x11, 0xbd, 0x84);
    public IReadOnlyList<IReadOnlyList<LevelSquare>> LevelSquares
    {
        get
        {
            List<LevelSquare> layouts = [];
            List<IReadOnlyList<LevelSquare>> layoutRows = [];
            for (int i = 0; i < MaxLevel; i++)
            {
                if (i % 10 == 0)
                {
                    layouts = [];
                    layoutRows.Add(layouts);
                }
                layouts.Add(new LevelSquare(i < Level, i < 10 ? SkillColor1 : SkillColor2));
            }
            return layoutRows;
        }
    }
    public string ExpToNextLayout =>
        $"{(ExpToNext == 0 ? 0 : (expToNextMax == 0 ? 0 : 100f * MathF.Min(ExpToNext, expToNextMax) / expToNextMax))}% stretch";
    public Color ExpToNextTint => Level < 10 ? SkillColor1 : SkillColor2;
    public string ExpToNextDisplay => I18n.Ui_Fulfillment_Dipslay(ExpToNext, expToNextMax);

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
            int lvlExp = Math.Max(GetSkillExperienceForLevel(level), 0);
            ExpToNext = exp - lvlExp;
            expToNextMax = Math.Max(GetSkillExperienceForLevel(level + 1) - lvlExp, 0);
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
                if (level >= 11)
                    return vppApi.LevelExperiences[level - 11];
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
