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

    public record SpeedGroKind(ParsedItemData Info, float Amount)
    {
        public string Tooltip = $"{Info.DisplayName} ({Amount:P2})";
    }

    public List<SpeedGroKind> SpeedGroKinds =>
        field ??= [
            new(ItemRegistry.GetDataOrErrorItem("(O)368"), 0f),
            new(ItemRegistry.GetDataOrErrorItem("(O)465"), 0.1f),
            new(ItemRegistry.GetDataOrErrorItem("(O)466"), 0.25f),
            new(ItemRegistry.GetDataOrErrorItem("(O)918"), 0.33f),
        ];

    public string LabelAgriculturist => Game1.content.LoadString("Strings/UI:LevelUp_ProfessionName_Agriculturist");

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
    public sealed record CropDay(SDUISprite? Sprite, bool IsHarvest, bool IsPaddy)
    {
        public bool ShowDirt = Sprite != null && !IsHarvest;
        public bool ShowPaddy => ShowDirt && IsPaddy;

        public int DayOfMonth { get; set; } = 0;
        public float DisplayShadow => IsHarvest ? 0.35f : 0f;
        public Color CellBorderTint
        {
            get
            {
                int today = Game1.dayOfMonth - 1;
                if (DayOfMonth < today)
                    return Color.Gray * 0.25f;
                return Color.White;
            }
        }
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

        GetAdjustedGrowDays(crop, settings.GetBoost(crop), out int growDays, out List<int> daysInPhase);

        int regrowDays = crop.RegrowDays;
        int phase = -1;
        UpdatePhase(daysInPhase, 0, ref phase, out int nextPhaseDay);
        int maxMonths = (int)MathF.Ceiling((float)(growDays + 1) / WorldDate.DaysPerMonth);
        int daysToShow = maxMonths * WorldDate.DaysPerMonth;

        List<IReadOnlyList<CropDay>> harvestCells = [];
        List<CropDay> harvestCellsMonth = [];

        if (regrowDays < 1)
        {
            // non-regrowing
            AddToHarvestCells(ref harvestCellsMonth, harvestCells, GetPhaseSprite(crop, cropTx, phase, 0));
            for (int day = 1; day < daysToShow; day++)
            {
                if (day >= nextPhaseDay)
                    UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                CropDay cropDay;
                if (day % growDays == 0)
                {
                    cropDay = GetHarvestSprite(info.Datum, harvestCellsMonth.Count);
                    phase = -1;
                    UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                }
                else
                {
                    cropDay = GetPhaseSprite(crop, cropTx, phase, harvestCellsMonth.Count);
                }
                AddToHarvestCells(ref harvestCellsMonth, harvestCells, cropDay);
            }
        }
        else
        {
            // regrowing
            AddToHarvestCells(ref harvestCellsMonth, harvestCells, GetPhaseSprite(crop, cropTx, phase, 0));
            for (int day = 1; day < growDays; day++)
            {
                if (day >= nextPhaseDay)
                    UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                AddToHarvestCells(
                    ref harvestCellsMonth,
                    harvestCells,
                    GetPhaseSprite(crop, cropTx, phase, harvestCells.Count)
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
                        : GetPhaseSprite(crop, cropTx, phase, harvestCellsMonth.Count)
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

    private static void GetAdjustedGrowDays(
        CropData crop,
        float speedBoost,
        out int growDays,
        out List<int> daysInPhase
    )
    {
        daysInPhase = crop.DaysInPhase.ToList();
        growDays = daysInPhase.Sum();
        int speedGroDays = (int)Math.Ceiling(growDays * speedBoost);
        int phaseIdx = 0;
        while (speedGroDays > 0 && phaseIdx < 3)
        {
            for (int j = 0; j < daysInPhase.Count; j++)
            {
                if ((j > 0 || daysInPhase[j] > 1) && daysInPhase[j] > 0)
                {
                    daysInPhase[j]--;
                    speedGroDays--;
                }
                if (speedGroDays <= 0)
                {
                    break;
                }
            }
            phaseIdx++;
        }
        growDays = daysInPhase.Sum();
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
                cropDay.Add(new(null, false, false) { DayOfMonth = i });
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

    private static void UpdatePhase(List<int> daysInPhase, int day, ref int phase, out int nextPhaseDay)
    {
        phase++;
        while (phase < daysInPhase.Count && daysInPhase[phase] == 0)
            phase++;
        if (phase >= daysInPhase.Count)
            phase = 0;
        nextPhaseDay = day + daysInPhase[phase];
    }

    private static CropDay GetPhaseSprite(CropData crop, Texture2D cropTx, int phase, int day)
    {
        int spriteIndex = crop.SpriteIndex;
        return new(
            new(
                cropTx,
                new(spriteIndex % 2 * 128 + (phase == 0 ? day % 2 : phase + 1) * 16, spriteIndex / 2 * 32, 16, 32),
                FixedEdges: SDUIEdges.NONE,
                SliceSettings: new(Scale: 3)
            ),
            false,
            crop.IsPaddyCrop
        );
    }

    private static CropDay GetHarvestSprite(ParsedItemData datum, int day)
    {
        return new(
            new(datum.GetTexture(), datum.GetSourceRect(), FixedEdges: SDUIEdges.NONE, SliceSettings: new(Scale: 3)),
            true,
            false
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

public sealed partial class GoalCropListContext(IGoalContext goalCtx, CropListKind kind)
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

    protected override IReadOnlyList<ShippedCountDisplay> SortAllDisplay(List<ShippedCountDisplay> displayList)
    {
        IReadOnlyList<ShippedCountDisplay> sorted = base.SortAllDisplay(displayList);
        if (sorted.Any())
            Hovered = sorted[0];
        return sorted;
    }

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
                _ => int.MaxValue,
            },
            cropCalendarSettings
        );
}
