using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfectionHandbook.GUI.Shared;
using PerfectionHandbook.Integration;
using PerfectionHandbook.Models;
using PerfectionHandbook.Reminders;
using PropertyChanged.SourceGenerator;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace PerfectionHandbook.GUI;

public sealed record CatchInSeason(SDUISprite Sprite, Color DisplayTint);

public sealed record CatchInTimeRange(string Text, bool WithinTime);

public sealed record CatchInDisplay(
    string LocationId,
    string LocationName,
    bool CatchableToday,
    HashSet<Season> SpawnSeasons,
    HashSet<string> SpawnWeather,
    HashSet<(int, int)> SpawnTimeRange,
    int SpawnMinFishingLevel,
    string? CrabPot = null
)
{
    public ParsedItemData CrabPotIcon => ItemRegistry.GetData("(O)710");

    public CatchInDisplay(string LocationId, string LocationName)
        : this(LocationId, LocationName, true, [], [], [(0600, 2600)], 0) { }

    public CatchInDisplay(string LocationId, string LocationName, string CrabPot)
        : this(LocationId, LocationName, true, [], [], [], 0, CrabPot: CrabPot) { }

    public readonly float Opacity = CatchableToday ? 1f : 0.4f;
    public readonly bool HasSpawnWeather = SpawnWeather.Any();
    public readonly bool HasSpawnMinFishingLevel = SpawnMinFishingLevel > 0;
    public readonly bool IsCrabPot = CrabPot != null;

    private static string FormatTime(int timeCode)
    {
        int hour = timeCode / 100;
        if (hour > 24)
            hour -= 24;
        return $"{hour:D2}:{timeCode % 100:D2}";
    }

    public IEnumerable<CatchInSeason> SpawnSeasonSprites
    {
        get
        {
            foreach (SeasonSprite seasonSprite in DrawHelper.SeasonSprites)
            {
                yield return new(
                    seasonSprite.Sprite,
                    SpawnSeasons.Count == 0 || SpawnSeasons.Contains(seasonSprite.Ssn)
                        ? HandbookContext.ActiveColor
                        : HandbookContext.InactiveColor
                );
            }
        }
    }
    public IEnumerable<SDUISprite> SpawnWeatherSprites
    {
        get
        {
            foreach (string weather in SpawnWeather)
            {
                if (DrawHelper.GetWeatherSprite(weather) is SDUISprite weatherSprite)
                {
                    yield return weatherSprite;
                }
            }
        }
    }
    public IEnumerable<CatchInTimeRange> SpawnTimeRangeText
    {
        get
        {
            foreach ((int startTime, int endTime) in SpawnTimeRange)
            {
                string text = I18n.Ui_FishTimeRange(FormatTime(startTime), FormatTime(endTime));
                yield return new(text, CatchableToday && startTime <= Game1.timeOfDay && endTime >= Game1.timeOfDay);
            }
        }
    }

    public static void MakeOrMerge(
        Dictionary<string, CatchInDisplay> canCatchIn,
        LocationInfo locInfo,
        SpawnFishData spawnFish,
        SpawnFishParsedReq? spawnReq
    )
    {
        if (!locInfo.HasWater)
            return;
        if (spawnFish.RequireMagicBait)
            return;

        bool catchableToday = true;
        bool? rain = spawnReq?.Rain;
        if (spawnFish.Season != null && spawnFish.Season != Game1.GetSeasonForLocation(locInfo.Location))
        {
            catchableToday = false;
        }
        else if (rain.HasValue && rain.Value != locInfo.Location.IsRainingHere())
        {
            catchableToday = false;
        }
        else if (
            !GameQueryHelper.ContextLocationCheck(spawnFish.Condition, locInfo.Location, GameQueryHelper.fishIgnoreKeys)
        )
        {
            catchableToday = false;
        }

        GameStateQuery.ParsedGameStateQuery[] conditions = string.IsNullOrEmpty(spawnFish.Condition)
            ? []
            : GameStateQuery.Parse(spawnFish.Condition);

        HashSet<Season> seasons = [];
        HashSet<string> weather = [];
        HashSet<(int, int)> timeRanges = [];

        if (spawnFish.Season != null)
        {
            seasons.Add(spawnFish.Season.Value);
        }

        foreach (GameStateQuery.ParsedGameStateQuery cond in conditions)
        {
            string query = cond.Query[0];
            if (query == "SEASON")
            {
                foreach (string seasonStr in cond.Query.Skip(1))
                {
                    if (Enum.TryParse(seasonStr, out Season season))
                        seasons.Add(season);
                }
            }
            else if (query == "LOCATION_SEASON")
            {
                foreach (string seasonStr in cond.Query.Skip(2))
                {
                    if (Enum.TryParse(seasonStr, ignoreCase: true, out Season season))
                        seasons.Add(season);
                }
            }
            else if (query == "WEATHER")
            {
                foreach (string seasonStr in cond.Query.Skip(2))
                {
                    weather.Add(seasonStr.ToLower());
                }
            }
            else if (query == "TIME")
            {
                if (
                    ArgUtility.TryGetInt(cond.Query, 1, out int minTime, out _, "int minTime")
                    && ArgUtility.TryGetOptionalInt(cond.Query, 2, out int maxTime, out _, int.MaxValue, "int maxTime")
                )
                {
                    timeRanges.Add((minTime, maxTime));
                }
            }
        }

        if (spawnReq != null && !spawnFish.IgnoreFishDataRequirements)
        {
            if (weather.Count == 0 && rain.HasValue)
            {
                if (rain.Value)
                {
                    weather.Add("rain");
                    weather.Add("storm");
                    weather.Add("greenrain");
                }
                else
                {
                    weather.Add("sun");
                }
            }
            if (timeRanges.Count == 0)
                timeRanges.AddRange(spawnReq.TimeRanges);
        }

        int minFishingLevel = Math.Max(spawnFish?.MinFishingLevel ?? 0, spawnReq?.MinFishing ?? 0);

        if (canCatchIn.TryGetValue(locInfo.Location.NameOrUniqueName, out CatchInDisplay? existingCanCatchIn))
        {
            existingCanCatchIn.SpawnSeasons.UnionWith(seasons);
            existingCanCatchIn.SpawnWeather.UnionWith(weather);
            existingCanCatchIn.SpawnTimeRange.UnionWith(timeRanges);
            canCatchIn[locInfo.Location.NameOrUniqueName] = new(
                locInfo.Location.NameOrUniqueName,
                existingCanCatchIn.LocationName,
                existingCanCatchIn.CatchableToday || catchableToday,
                existingCanCatchIn.SpawnSeasons,
                existingCanCatchIn.SpawnWeather,
                existingCanCatchIn.SpawnTimeRange,
                Math.Min(minFishingLevel, existingCanCatchIn.SpawnMinFishingLevel)
            );
        }
        else
        {
            canCatchIn[locInfo.Location.NameOrUniqueName] = new(
                locInfo.Location.NameOrUniqueName,
                locInfo.Location.DisplayName ?? locInfo.LocationId,
                catchableToday,
                seasons,
                weather,
                timeRanges.Any() ? timeRanges : [(0600, 2600)],
                Math.Max(spawnFish?.MinFishingLevel ?? 0, spawnReq?.MinFishing ?? 0)
            );
        }
    }
}

public sealed record FishCaughtDisplay(ItemInfo Info, int OwnedCount) : AbstractItemCountDisplay(Info, OwnedCount)
{
    public override string FocusableTag { get; } = $"fish-{Info.Datum.QualifiedItemId}";

    public override bool Needed => Count < 0;
    private int biggestCatch = 0;
    public IReadOnlyList<CatchInDisplay> CanCatchIn { get; set; } = [];

    public override Color DisplayTint
    {
        get
        {
            if (countMode == CountMode.Owned)
            {
                return base.DisplayTint;
            }
            return CanCatchIn.Any(cci => cci.CatchableToday)
                ? HandbookContext.ActiveColor
                : HandbookContext.HiddenColor;
        }
    }

    public override void SetStatus(Farmer who)
    {
        if (who.fishCaught.TryGetValue(Info.Datum.QualifiedItemId, out int[] pair))
        {
            completedCount = pair[0];
            biggestCatch = pair[1];
        }
        else
        {
            completedCount = -1;
            biggestCatch = 0;
        }
        Count = completedCount;
        OnPropertyChanged(new(nameof(Tooltip)));
    }

    private static readonly StringBuilder sb = new();

    public override string GetTooltipDesc()
    {
        sb.Append(Info.Datum.Description);
        if (Count > 0)
        {
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(
                I18n.Ui_FishCatch(Count, biggestCatch > 0 ? I18n.Ui_FishCatchLength(biggestCatch) : string.Empty)
            );
        }
        string result = sb.ToString();
        sb.Clear();
        return result;
    }

    public override ReminderEntry? Reminder { get; } =
        MenuHandler.Reminders.GetOrCreateEntry(ReminderEntryFactory.Kind_FishCaught, Info.ReprItem.QualifiedItemId);
}

public sealed partial class GoalFishCaughtContext(IGoalContext goalCtx)
    : AbstractItemCountContext<FishCaughtDisplay>(
        goalCtx,
        canToggleNeeded: true,
        canToggleCountMode: true,
        defaultCountMode: CountMode.Completed,
        itemPerPageModifier: 7.0 / 8.0
    )
{
    public override string CompleteCountToggleText => I18n.Ui_CountingFished();

    protected override bool ShouldInclude(ItemInfo itemInfo) => itemInfo.IsCatchableFish;

    protected override FishCaughtDisplay MakeDisplay(ItemInfo itemInfo, int ownedCount) => new(itemInfo, ownedCount);

    protected override List<FishCaughtDisplay> FinalizeDisplay(List<FishCaughtDisplay> displayList)
    {
        displayList = base.FinalizeDisplay(displayList);
        foreach (FishCaughtDisplay disp in displayList)
        {
            Dictionary<string, CatchInDisplay> canCatchIn = [];
            foreach ((LocationInfo locInfo, SpawnFishData spawn) in disp.Info.FromFishing)
            {
                CatchInDisplay.MakeOrMerge(canCatchIn, locInfo, spawn, disp.Info.FishReq);
            }

            // mines fish hardcoding
            switch (disp.Info.Datum.QualifiedItemId)
            {
                case "(O)158":
                    canCatchIn[$"{ModEntry.ModId}_mines_20"] = new(
                        $"{ModEntry.ModId}_mines_20",
                        I18n.Ui_Mines_Floor(20)
                    );
                    break;
                case "(O)161":
                    canCatchIn[$"{ModEntry.ModId}_mines_60"] = new(
                        $"{ModEntry.ModId}_mines_60",
                        I18n.Ui_Mines_Floor(60)
                    );
                    break;
                case "(O)162":
                case "(O)CaveJelly":
                    canCatchIn[$"{ModEntry.ModId}_mines_100"] = new(
                        $"{ModEntry.ModId}_mines_100",
                        I18n.Ui_Mines_Floor(100)
                    );
                    break;
            }

            // crab pots
            if (disp.Info.FishReq?.CrabPotGroups?.Any() ?? false)
            {
                foreach (string crabPot in disp.Info.FishReq.CrabPotGroups)
                {
                    string id = $"crabpot.{crabPot}";
                    string crabpotTl = id;
                    if (ModEntry.help.Translation.ContainsKey(crabpotTl))
                        crabpotTl = ModEntry.help.Translation.Get(crabpotTl);
                    else
                        crabpotTl = crabPot;
                    canCatchIn[id] = new(id, id, crabpotTl);
                    break;
                }
            }

            List<CatchInDisplay> canCatchInLst = canCatchIn
                .Values.OrderBy(cci => (cci.CatchableToday ? 0 : 1, !cci.IsCrabPot ? 0 : 1, cci.LocationName))
                .ToList();
            disp.CanCatchIn = canCatchInLst;
        }
        return displayList;
    }

    protected override List<FishCaughtDisplay> SortAllDisplay(List<FishCaughtDisplay> displayList)
    {
        if (SortMode == SORTMODE_DEFAULT)
        {
            return displayList
                .OrderBy(static disp =>
                    (
                        disp.CanCatchIn.Any(cci => cci.CatchableToday) ? 0 : 1,
                        disp.CanCatchIn.Any(cci => !cci.IsCrabPot) ? 0 : 1,
                        disp.Info.Datum.Category,
                        disp.Info.Datum.QualifiedItemId
                    )
                )
                .ToList();
        }
        return base.SortAllDisplay(displayList);
    }

    public void ToggleHoverable(FishCaughtDisplay display)
    {
        if (display.ToggleReminder())
            return;
        HoveredClick(display);
    }

    public override bool TryOpenPage()
    {
        bool result = base.TryOpenPage();
        if (result)
        {
            if (FilteredDisplayPaginated.Count > 0)
                DoHoveredEnter(FilteredDisplayPaginated[0]);
        }
        return result;
    }

    #region tank fish rendering
    private readonly SpriteBatch tankfishBatch = new(Game1.graphics.GraphicsDevice);
    private const int TANK_WIDTH = 328;
    private const int TANK_HEIGHT = 80;
    private readonly RenderTarget2D tankfishRT = new(
        Game1.graphics.GraphicsDevice,
        TANK_WIDTH,
        TANK_HEIGHT,
        false,
        SurfaceFormat.Color,
        DepthFormat.None,
        0,
        RenderTargetUsage.DiscardContents
    );

    [Notify]
    private TankFish? hoveredTankFish = null;
    public bool HasHoveredTankFishSprite => HoveredTankFish != null;
    public SDUISprite HoveredTankFishSprite =>
        new(tankfishRT, tankfishRT.Bounds, SDUIEdges.NONE, SliceSettings: new(Scale: 1f));

    private sealed class BogusFishTank() : FishTankFurniture("CCFishTank", Vector2.One)
    {
        public override Rectangle GetTankBounds()
        {
            return new(Game1.viewport.X, Game1.viewport.Y, TANK_WIDTH, TANK_HEIGHT);
        }
    }

    protected override void DoHoveredEnter(FishCaughtDisplay display)
    {
        if (display.ReprItem.ItemId != HoveredTankFish?.fishItemId)
        {
            TankFish newTankfish = new(new BogusFishTank(), display.ReprItem);
            if (newTankfish.isErrorFish)
                HoveredTankFish = null;
            else
                HoveredTankFish = newTankfish;
        }
        base.DoHoveredEnter(display);
    }

    public void Update(TimeSpan timeSpan)
    {
        if (hoveredTankFish == null)
            return;
        hoveredTankFish.Update(Game1.currentGameTime);
        hoveredTankFish.position.Y = TANK_HEIGHT;
        if (hoveredTankFish.fishType == TankFish.FishType.Crawl)
            hoveredTankFish.position.Y -= TANK_HEIGHT / 4;
        hoveredTankFish.zPosition = -TANK_HEIGHT / 8;
        DrawHelper.RenderToTarget(
            tankfishRT,
            (renderBatch) => hoveredTankFish.Draw(renderBatch, 1f, 1f),
            tankfishBatch
        );
    }
    #endregion
}
