using System.ComponentModel;
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

public sealed partial class CropDetailDisplaySettings
{
    public int StartDay { get; set; } = Game1.dayOfMonth - 1;

    [Notify]
    private int speedGroIdx = 0;

    public record SpeedGroKind(ParsedItemData Info, float Amount);

    public List<SpeedGroKind> SpeedGroKinds =>
        field ??= [
            new(ItemRegistry.GetDataOrErrorItem("(O)368"), 0f),
            new(ItemRegistry.GetDataOrErrorItem("(O)465"), 0.1f),
            new(ItemRegistry.GetDataOrErrorItem("(O)466"), 0.25f),
            new(ItemRegistry.GetDataOrErrorItem("(O)918"), 0.33f),
        ];

    [Notify]
    public bool useAgriculturist = false;

    public bool UsePaddy { get; set; } = true;

    public float GetBoost(CropData crop)
    {
        return SpeedGroKinds[SpeedGroIdx].Amount
            + (UsePaddy && crop.IsPaddyCrop ? 0.25f : 0f)
            + (UseAgriculturist ? 0.1f : 0f);
    }
}

public sealed partial class CropDetailDisplay
{
    public sealed record CropDay(SDUISprite? Sprite, bool IsHarvest)
    {
        public bool HasSprite = Sprite != null;
        public int DayOfMonth { get; set; } = 0;
        public float DisplayShadow => IsHarvest ? 0.35f : 0f;
    }

    public readonly ItemInfo Info;
    public readonly CropDetailDisplaySettings Settings;
    public IReadOnlyList<IReadOnlyList<CropDay>> AllHarvestCells;

    public CropDetailDisplay(ItemInfo info, CropDetailDisplaySettings settings)
    {
        Info = info;
        Settings = settings;
        AllHarvestCells = MakeAllHarvestCells(info, settings);
        settings.PropertyChanged += OnDisplaySettingsChanged;
    }

    private void OnDisplaySettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshAllHarvestCells();
        OnPropertyChanged(new(nameof(Month)));
        OnPropertyChanged(new(nameof(HarvestCells)));
    }

    private static IReadOnlyList<IReadOnlyList<CropDay>> MakeAllHarvestCells(
        ItemInfo info,
        CropDetailDisplaySettings settings
    )
    {
        CropData crop = info.FromCrop[0];
        Texture2D cropTx = DrawHelper.SafeLoad(crop.Texture, Game1.cropSpriteSheet);

        int growDays = GetAdjustedGrowDays(crop, settings.GetBoost(crop));

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
                harvestcells.Add(harvestCellsMonth);
                harvestCellsMonth = [];
            }
        }
    }

    public void RefreshAllHarvestCells()
    {
        AllHarvestCells = MakeAllHarvestCells(Info, Settings);
    }

    private static int GetAdjustedGrowDays(CropData crop, float speedBoost)
    {
        List<int> phaseDays = [.. crop.DaysInPhase];
        int growDays = crop.DaysInPhase.Sum();
        int speedGroDays = (int)Math.Ceiling(growDays * speedBoost);
        int phaseIdx = 0;
        while (speedGroDays > 0 && phaseIdx < 3)
        {
            for (int j = 0; j < phaseDays.Count; j++)
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
            IReadOnlyList<CropDay> harvestCellsMonth = AllHarvestCells[0];
            List<CropDay> cropDay = [];
            for (int i = 0; i < Settings.StartDay; i++)
                cropDay.Add(new(null, false) { DayOfMonth = i });
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
            ),
            false
        );
    }

    private static CropDay GetHarvestSprite(ParsedItemData datum, int day)
    {
        return new(
            new(datum.GetTexture(), datum.GetSourceRect(), FixedEdges: SDUIEdges.NONE, SliceSettings: new(Scale: 3)),
            true
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
    int OwnedCount,
    int RequiredCount,
    CropDetailDisplaySettings CropCalendarSettings
) : ItemShippedDisplay(Info, OwnedCount)
{
    public override bool Needed => completedCount <= RequiredCount;

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

    public CropDetailDisplay CropDetail => field ??= new(Info, CropCalendarSettings);
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
    public override string CompleteCountToggleText => I18n.Ui_CountingShipped();

    protected override bool ShouldInclude(ItemInfo itemInfo) =>
        kind switch
        {
            CropListKind.Monoculture => itemInfo.CountForMonoculture,
            CropListKind.Polyculture => itemInfo.CountForPolyculture,
            _ => itemInfo.FromCrop.Any(),
        };

    protected override void UpdateDisplayingFulfillment(GoalFulfillment fulfillment)
    {
        base.UpdateDisplayingFulfillment(fulfillment);
        cropCalendarSettings.UseAgriculturist = fulfillment.Who?.professions.Contains(Farmer.agriculturist) ?? false;
    }

    [Notify]
    private bool hoverable = true;

    public void ToggleHoverable(ShippedCountDisplay display)
    {
        if (Hoverable)
        {
            Hoverable = false;
            base.HoveredEnter(display);
            Hovered?.BorderTint = Color.White;
        }
        else
        {
            if (display == Hovered)
            {
                Hoverable = true;
                Hovered?.BorderTint = Color.Transparent;
            }
            else
            {
                Hovered?.BorderTint = Color.Transparent;
                base.HoveredEnter(display);
                Hovered?.BorderTint = Color.White;
            }
        }
    }

    public override void HoveredEnter(ShippedCountDisplay display)
    {
        if (Hoverable)
            base.HoveredEnter(display);
    }

    private readonly CropDetailDisplaySettings cropCalendarSettings = new();

    protected override ShippedCountDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) =>
        new(
            itemInfo,
            ownedCount,
            kind switch
            {
                CropListKind.Monoculture => 300,
                CropListKind.Polyculture => 15,
                _ => 0,
            },
            cropCalendarSettings
        );
}
