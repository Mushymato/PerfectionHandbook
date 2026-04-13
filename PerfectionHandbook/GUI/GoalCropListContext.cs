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
    public int StartDay { get; set; } = 0;

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

    public string LabelAgriculturist =>
        $"{Game1.content.LoadString("Strings/UI:LevelUp_ProfessionName_Agriculturist")} ({0.1f:P2})";

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
    public ParsedItemData Basket => ItemRegistry.GetDataOrErrorItem("(O)790");

    public sealed record CropDay(SDUISprite? Sprite, bool IsHarvest, bool IsPaddy)
    {
        public bool ShowDirt = Sprite != null && !IsHarvest;
        public bool ShowPaddy => ShowDirt && IsPaddy;

        public int Day { get; set; } = 0;
        public Season ThisSeason { get; set; } = Season.Spring;
        public float DisplayShadow => IsHarvest ? 0.35f : 0f;
        public Color CellBorderTint
        {
            get
            {
                if (ThisSeason < Game1.season)
                    return Color.Gray * 0.25f;
                int today = Game1.dayOfMonth - 1;
                if (ThisSeason == Game1.season && Day % WorldDate.DaysPerMonth < today)
                    return Color.Gray * 0.25f;
                return Color.White;
            }
        }
    }

    public readonly ItemInfo HarvestInfo;
    public readonly ParsedItemData Seed;
    public readonly CropData Crop;
    public readonly CropDetailDisplaySettings Settings;

    [Notify]
    private int month = 0;
    public readonly List<Season> CropSeasons;
    public IReadOnlyList<SeasonSprite> CropSeasonSprites =>
        CropSeasons.Select(season => DrawHelper.GetSeasonSprite(season)).ToList();

    public CropDetailDisplay(ItemInfo harvestInfo, CropDetailDisplaySettings settings)
    {
        HarvestInfo = harvestInfo;
        Settings = settings;
        KeyValuePair<string, CropData> cropPair = harvestInfo.FromCrop.FirstOrDefault();
        Seed = ItemRegistry.GetDataOrErrorItem(cropPair.Key);
        Crop = cropPair.Value;
        CropSeasons = Crop.Seasons?.Any() ?? false ? Crop.Seasons : [Game1.season];
        CropSeasons.Sort();
        if (CropSeasons.Count < 4 && CropSeasons[0] == Season.Spring && CropSeasons[^1] == Season.Winter)
        {
            CropSeasons.RemoveAt(0);
            CropSeasons.Add(Season.Spring);
        }
        int month = CropSeasons.IndexOf(Game1.season);
        Month = month > -1 ? month : 0;

        settings.PropertyChanged += OnDisplaySettingsChanged;
    }

    private void OnDisplaySettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        allHarvestCells = null;
        OnPropertyChanged(new(nameof(Month)));
        OnPropertyChanged(new(nameof(HarvestCells)));
    }

    public IReadOnlyList<CropDay>? allHarvestCells = null;
    public IReadOnlyList<CropDay> AllHarvestCells =>
        allHarvestCells ??= MakeAllHarvestCells(Crop, CropSeasons, HarvestInfo.Datum, Settings);

    private static IReadOnlyList<CropDay> MakeAllHarvestCells(
        CropData crop,
        List<Season> cropSeasons,
        ParsedItemData harvestItem,
        CropDetailDisplaySettings settings
    )
    {
        Texture2D cropTx = DrawHelper.SafeLoad(crop.Texture, Game1.cropSpriteSheet);

        GetAdjustedGrowDays(crop, settings.GetBoost(crop), out int growDays, out List<int> daysInPhase);

        int regrowDays = crop.RegrowDays;
        int phase = -1;
        UpdatePhase(daysInPhase, 0, ref phase, out int nextPhaseDay);

        List<CropDay> seasonCropDay = [];
        Season? prevSeason = null;
        int contDay = 0;
        foreach (Season season in cropSeasons)
        {
            if (prevSeason != null)
            {
                if (season == Season.Spring)
                {
                    if (prevSeason != Season.Winter)
                        contDay = 0;
                }
                else if (season != prevSeason + 1)
                {
                    contDay = 0;
                }
            }
            if (regrowDays < 1)
            {
                // non-regrowing
                int startContDay = contDay;
                for (int day = 0; day < WorldDate.DaysPerMonth; day++)
                {
                    CropDay cropDay;
                    if ((day + startContDay) % growDays == 0 && contDay != 0)
                    {
                        cropDay = GetHarvestSprite(harvestItem, seasonCropDay.Count);
                        phase = -1;
                        UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                    }
                    else
                    {
                        cropDay = GetPhaseSprite(crop, cropTx, phase, seasonCropDay.Count);
                    }
                    AddCropDay(ref contDay, seasonCropDay, cropDay);
                    if (day >= nextPhaseDay)
                        UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                }
            }
            else
            {
                // regrowing
                if (contDay == 0)
                {
                    for (int day = 0; day < growDays; day++)
                    {
                        AddCropDay(
                            ref contDay,
                            seasonCropDay,
                            GetPhaseSprite(crop, cropTx, phase, seasonCropDay.Count)
                        );
                        if (day >= nextPhaseDay)
                            UpdatePhase(daysInPhase, day, ref phase, out nextPhaseDay);
                    }
                    int matureDay = contDay;
                    AddCropDay(ref contDay, seasonCropDay, GetHarvestSprite(harvestItem, seasonCropDay.Count));
                    for (int day = 1; day < WorldDate.DaysPerMonth - growDays; day++)
                    {
                        seasonCropDay.Add(
                            day % regrowDays == 0
                                ? GetHarvestSprite(harvestItem, seasonCropDay.Count)
                                : GetPhaseSprite(crop, cropTx, phase, seasonCropDay.Count)
                        );
                    }
                }
                else
                {
                    for (int day = 0; day < WorldDate.DaysPerMonth; day++)
                    {
                        seasonCropDay.Add(
                            (day + contDay) % regrowDays == 0
                                ? GetHarvestSprite(harvestItem, seasonCropDay.Count)
                                : GetPhaseSprite(crop, cropTx, phase, seasonCropDay.Count)
                        );
                    }
                }
            }
            prevSeason = season;
        }

        return seasonCropDay;

        static void AddCropDay(ref int contDay, List<CropDay> seasonCropDay, CropDay cropDay)
        {
            seasonCropDay.Add(cropDay);
            contDay++;
        }

        static CropDay GetPhaseSprite(CropData crop, Texture2D cropTx, int phase, int day)
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

        static CropDay GetHarvestSprite(ParsedItemData datum, int _)
        {
            return new(
                new(
                    datum.GetTexture(),
                    datum.GetSourceRect(),
                    FixedEdges: SDUIEdges.NONE,
                    SliceSettings: new(Scale: 3)
                ),
                true,
                false
            );
        }
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

    public IReadOnlyList<CropDay> HarvestCells
    {
        get
        {
            Season thisSeason = CropSeasons[Month];
            List<CropDay> cropDays = [];
            int startIdx = WorldDate.DaysPerMonth * Month;
            int startDay = Settings.StartDay;
            for (int i = startIdx; i < startIdx + WorldDate.DaysPerMonth; i++)
            {
                CropDay cropDay;
                if (i < startDay)
                {
                    cropDay = new(null, false, false) { Day = i, ThisSeason = thisSeason };
                }
                else
                {
                    cropDay = AllHarvestCells[i - startDay];
                    cropDay.Day = i;
                    cropDay.ThisSeason = thisSeason;
                }
                cropDays.Add(cropDay);
            }
            return cropDays;
        }
    }

    private static void UpdatePhase(List<int> daysInPhase, int day, ref int phase, out int nextPhaseDay)
    {
        phase++;
        while (phase < daysInPhase.Count && daysInPhase[phase] == 0)
            phase++;
        if (phase >= daysInPhase.Count)
            phase = 0;
        nextPhaseDay = day + daysInPhase[phase];
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
        if (Month < CropSeasons.Count - 1)
            Month++;
    }

    private void PrevMonth()
    {
        if (Month > 0)
            Month--;
    }

    public void ChangeStartDay(CropDay cropDay)
    {
        if (Settings.StartDay != cropDay.Day)
        {
            Settings.StartDay = cropDay.Day;
            OnPropertyChanged(new(nameof(HarvestCells)));
        }
    }
}

public sealed partial record CropDisplay(
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
    : AbstractItemCountContext<CropDisplay>(goalCtx)
{
    public override string CompleteCountToggleText => I18n.Ui_CountingShipped();

    protected override bool ShouldInclude(ItemInfo itemInfo) =>
        kind switch
        {
            CropListKind.Monoculture => itemInfo.CountForMonoculture,
            CropListKind.Polyculture => itemInfo.CountForPolyculture,
            _ => itemInfo.FromCrop.Any(),
        };

    protected override IReadOnlyList<CropDisplay> SortAllDisplay(List<CropDisplay> displayList)
    {
        IReadOnlyList<CropDisplay> sorted = displayList
            .OrderBy(static disp =>
            {
                Season firstSeason = disp.CropDetail.CropSeasons.First();
                return (firstSeason, disp.Info.Datum.Category, disp.Info.Datum.QualifiedItemId);
            })
            .ToList();
        if (sorted.Any())
            Hovered = sorted.FirstOrDefault(info => info.CropDetail.CropSeasons.Contains(Game1.season)) ?? sorted[0];
        return sorted;
    }

    protected override void UpdateDisplayingFulfillment(GoalFulfillment fulfillment)
    {
        base.UpdateDisplayingFulfillment(fulfillment);
        cropCalendarSettings.UseAgriculturist = fulfillment.Who?.professions.Contains(Farmer.agriculturist) ?? false;
    }

    [Notify]
    private bool hoverable = true;

    public void ToggleHoverable(CropDisplay display)
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

    public override void HoveredEnter(CropDisplay display)
    {
        if (Hoverable)
            base.HoveredEnter(display);
    }

    private readonly CropDetailDisplaySettings cropCalendarSettings = new();

    protected override CropDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) =>
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
