using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.ItemTypeDefinitions;

namespace PerfectionHandbook.GUI;

public sealed class CropDetailDisplaySettings
{
    public int StartDay { get; set; } = Game1.dayOfMonth - 1;

    public float SpeedGro { get; set; } = 0;
    public bool UsePaddy { get; set; } = true;
    public bool UseAgriculturist { get; set; } = false;

    public float GetBoost(CropData crop) =>
        SpeedGro + (UsePaddy && crop.IsPaddyCrop ? 0.25f : 0f) + (UseAgriculturist ? 0.1f : 0f);
}

public sealed partial record CropDetailDisplay(ItemInfo Info, CropDetailDisplaySettings Settings)
{
    public sealed record CropDay(SDUISprite? Sprite)
    {
        public bool HasSprite = Sprite != null;
        public int DayOfMonth { get; set; } = 0;
    }

    public IReadOnlyList<IReadOnlyList<CropDay>> AllHarvestCells = MakeAllHarvestCells(Info, Settings);

    private static IReadOnlyList<IReadOnlyList<CropDay>> MakeAllHarvestCells(
        ItemInfo info,
        CropDetailDisplaySettings boost
    )
    {
        CropData crop = info.FromCrop[0];
        Texture2D cropTx = DrawHelper.SafeLoad(crop.Texture, Game1.cropSpriteSheet);

        int growDays = GetAdjustedGrowDays(crop, boost.GetBoost(crop));

        int regrowDays = crop.RegrowDays;
        int phase = -1;
        UpdatePhase(crop, 0, ref phase, out int nextPhaseDay);
        int maxMonths = (int)MathF.Ceiling((float)(growDays + 1) / WorldDate.DaysPerMonth);
        int daysToShow = maxMonths * WorldDate.DaysPerMonth;

        List<IReadOnlyList<CropDay>> harvestCells = [];
        List<CropDay> harvestCellsMonth = [];

        if (regrowDays < 1)
        {
            // non-regrowing
            AddToHarvestCells(ref harvestCellsMonth, harvestCells, GetPhaseSprite(cropTx, crop.SpriteIndex, phase, 0));
            for (int day = 1; day < daysToShow; day++)
            {
                if (day >= nextPhaseDay)
                    UpdatePhase(crop, day, ref phase, out nextPhaseDay);
                AddToHarvestCells(
                    ref harvestCellsMonth,
                    harvestCells,
                    day % growDays == 0
                        ? GetHarvestSprite(info.Datum, harvestCellsMonth.Count)
                        : GetPhaseSprite(cropTx, crop.SpriteIndex, phase, harvestCellsMonth.Count)
                );
            }
        }
        else
        {
            // regrowing
            AddToHarvestCells(ref harvestCellsMonth, harvestCells, GetPhaseSprite(cropTx, crop.SpriteIndex, phase, 0));
            for (int day = 1; day < growDays; day++)
            {
                if (day >= nextPhaseDay)
                    UpdatePhase(crop, day, ref phase, out nextPhaseDay);
                AddToHarvestCells(
                    ref harvestCellsMonth,
                    harvestCells,
                    GetPhaseSprite(cropTx, crop.SpriteIndex, phase, harvestCells.Count)
                );
            }
            AddToHarvestCells(ref harvestCellsMonth, harvestCells, GetHarvestSprite(info.Datum, 0));
            for (int day = 1; day < daysToShow - growDays; day++)
            {
                AddToHarvestCells(
                    ref harvestCellsMonth,
                    harvestCells,
                    day % regrowDays == 0
                        ? GetHarvestSprite(info.Datum, harvestCellsMonth.Count)
                        : GetPhaseSprite(cropTx, crop.SpriteIndex, phase, harvestCellsMonth.Count)
                );
            }
        }

        return harvestCells;

        static void AddToHarvestCells(
            ref List<CropDay> harvestCellsMonth,
            List<IReadOnlyList<CropDay>> harvestcells,
            CropDay cropDay
        )
        {
            harvestCellsMonth.Add(cropDay);
            if (harvestCellsMonth.Count == WorldDate.DaysPerMonth)
            {
                foreach (var cell in harvestCellsMonth)
                {
                    ModEntry.Log($"{cell.DayOfMonth}");
                }
                harvestcells.Add(harvestCellsMonth);
                harvestCellsMonth = [];
            }
        }
    }

    private static int GetAdjustedGrowDays(CropData crop, float speedBoost)
    {
        List<int> phaseDays = [.. crop.DaysInPhase];
        int growDays = 0;
        for (int i = 0; i < phaseDays.Count; i++)
        {
            growDays += phaseDays[i];
        }
        int speedGroDays = (int)Math.Ceiling((float)growDays * speedBoost);
        int phaseIdx = 0;
        while (speedGroDays > 0 && phaseIdx < 3)
        {
            for (int j = 0; j <= phaseDays.Count; j++)
            {
                if ((j > 0 || phaseDays[j] > 1) && phaseDays[j] > 0)
                {
                    phaseDays[j]--;
                    speedGroDays--;
                }
                if (speedGroDays <= 0)
                {
                    break;
                }
            }
            phaseIdx++;
        }
        return phaseDays.Sum();
    }

    [Notify]
    private int month = 0;

    public IReadOnlyList<CropDay> HarvestCells
    {
        get
        {
            ModEntry.Log($"HarvestCells: {Settings.StartDay}");
            IReadOnlyList<CropDay> harvestCellsMonth = AllHarvestCells[0];
            List<CropDay> cropDay = [];
            for (int i = 0; i < Settings.StartDay; i++)
                cropDay.Add(new(null) { DayOfMonth = i });
            for (int i = 0; i < WorldDate.DaysPerMonth - Settings.StartDay; i++)
            {
                harvestCellsMonth[i].DayOfMonth = Settings.StartDay + i;
                cropDay.Add(harvestCellsMonth[i]);
            }
            return cropDay;
        }
    }
    public bool ShowMonth => Month > 0;
    public string DisplayMonth => I18n.Ui_Crop_Month(Month);

    private static void UpdatePhase(CropData crop, int day, ref int phase, out int nextPhaseDay)
    {
        phase++;
        if (phase >= crop.DaysInPhase.Count)
            phase = 0;
        nextPhaseDay = day + crop.DaysInPhase[phase];
    }

    private static CropDay GetPhaseSprite(Texture2D cropTx, int spriteIndex, int phase, int day)
    {
        return new(
            new(
                cropTx,
                new(spriteIndex % 2 * 128 + (phase == 0 ? day % 2 : phase + 1) * 16, spriteIndex / 2 * 32, 16, 32),
                FixedEdges: SDUIEdges.NONE,
                SliceSettings: new(Scale: 3)
            )
        );
    }

    private static CropDay GetHarvestSprite(ParsedItemData datum, int day)
    {
        return new(
            new(datum.GetTexture(), datum.GetSourceRect(), FixedEdges: SDUIEdges.NONE, SliceSettings: new(Scale: 3))
        );
    }

    public void ScrollMonth(SDUIDirection direction)
    {
        if (direction == SDUIDirection.North)
            PrevMonth();
        else if (direction == SDUIDirection.South)
            NextMonth();
    }

    public void ChangeMonth(SButton button)
    {
        if (button == SButton.LeftShoulder)
            PrevMonth();
        else if (button == SButton.RightShoulder)
            NextMonth();
    }

    private void NextMonth()
    {
        if (Month < AllHarvestCells.Count - 1)
            Month++;
    }

    private void PrevMonth()
    {
        if (Month > 0)
            Month--;
    }

    public void ChangeStartDay(CropDay cropDay)
    {
        if (Settings.StartDay != cropDay.DayOfMonth)
        {
            Settings.StartDay = cropDay.DayOfMonth;
            OnPropertyChanged(new(nameof(HarvestCells)));
        }
    }
}

public sealed partial record ShippedCountDisplay(
    ItemInfo Info,
    ReprObject? OwnedRepr,
    int RequiredCount,
    CropDetailDisplaySettings CropCalendarSettings
) : AbstractItemCountDisplay(Info, OwnedRepr)
{
    public override bool Needed => Count < RequiredCount;

    public Color BorderTint
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(new(nameof(BorderTint)));
            }
        }
    }

    public override void SetStatus(Farmer who)
    {
        Count = who.basicShipped.GetValueOrDefault(Info.Datum.ItemId, 0);
        OwnedRepr?.SetReprStack(Count);
        DisplayTint = Count <= 0 ? HandbookContext.InactiveColor : HandbookContext.ActiveColor;
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    public override string GetTooltipDesc()
    {
        if (OwnedRepr == null)
            return Info.Datum.Description;
        return string.Concat(
            Info.Datum.Description,
            Environment.NewLine,
            Environment.NewLine,
            I18n.Ui_ShippedCount(OwnedRepr.ReprStack)
        );
    }

    public CropDetailDisplay CropDetail => new(Info, CropCalendarSettings);
}

public enum CropListKind
{
    Any,
    Monoculture,
    Polyculture,
}

public sealed partial class GoalCropListContext(GoalContext goalCtx, CropListKind kind)
    : AbstractItemCountContext<ShippedCountDisplay>(goalCtx)
{
    protected override bool ShouldInclude(ItemInfo itemInfo) =>
        kind switch
        {
            CropListKind.Monoculture => itemInfo.CountForMonoculture,
            CropListKind.Polyculture => itemInfo.CountForPolyculture,
            _ => itemInfo.FromCrop.Any(),
        };

    protected override ReprObject? GetReprObject(ItemInfo itemInfo) => new(itemInfo.ReprItem.getOne());

    [Notify]
    private bool hoverable = true;

    public void ToggleHoverable(ShippedCountDisplay display)
    {
        Hoverable = !Hoverable;
        Hovered?.BorderTint = Color.Transparent;
        if (Hoverable)
            return;
        base.HoveredEnter(display);
        Hovered?.BorderTint = Color.White;
    }

    public override void HoveredEnter(ShippedCountDisplay display)
    {
        if (Hoverable)
            base.HoveredEnter(display);
    }

    private readonly CropDetailDisplaySettings cropCalendarSettings = new();

    protected override ShippedCountDisplay MakeDisplay(ItemInfo itemInfo, ReprObject? ownedRepr) =>
        new(
            itemInfo,
            ownedRepr,
            kind switch
            {
                CropListKind.Monoculture => 300,
                CropListKind.Polyculture => 15,
                _ => 0,
            },
            cropCalendarSettings
        );
}
